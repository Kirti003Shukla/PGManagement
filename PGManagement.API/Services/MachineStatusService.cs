using Microsoft.EntityFrameworkCore;
using PGManagement.API.Data;
using PGManagement.API.DTOs;
using PGManagement.API.Models;

namespace PGManagement.API.Services;

public sealed class MachineStatusService : IMachineStatusService
{
    private readonly PGManagementDbContext _dbContext;

    public MachineStatusService(PGManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MachineStatusDto>> GetMachineStatusesAsync()
    {
        var nowUtc = DateTime.UtcNow;

        var machines = await _dbContext.Machines
            .OrderBy(m => m.MachineId)
            .ToListAsync();

        if (machines.Count == 0)
        {
            return new List<MachineStatusDto>();
        }

        var machineIds = machines.Select(m => m.MachineId).ToList();

        var activeSessions = await _dbContext.MachineSessions
            .Where(s => machineIds.Contains(s.MachineId) && s.Status == MachineSessionStatus.Active)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var machineById = machines.ToDictionary(m => m.MachineId);
        var activeSessionByMachineId = new Dictionary<int, MachineSession>();
        var stateChanged = false;

        foreach (var session in activeSessions)
        {
            if (session.EndTime.HasValue && session.EndTime.Value <= nowUtc)
            {
                session.Status = MachineSessionStatus.Completed;

                if (machineById.TryGetValue(session.MachineId, out var machine))
                {
                    machine.IsAvailable = true;
                    machine.CurrentUserId = null;
                    machine.EndTime = null;
                }

                stateChanged = true;
                continue;
            }

            if (!activeSessionByMachineId.ContainsKey(session.MachineId))
            {
                activeSessionByMachineId[session.MachineId] = session;
            }
        }

        if (stateChanged)
        {
            await _dbContext.SaveChangesAsync();
        }

        var result = new List<MachineStatusDto>(machines.Count);
        foreach (var machine in machines)
        {
            activeSessionByMachineId.TryGetValue(machine.MachineId, out var activeSession);
            var hasActiveSession = activeSession is not null;

            int? remainingMinutes = null;
            if (hasActiveSession)
            {
                var effectiveEndTime = activeSession!.EndTime ?? machine.EndTime;
                if (effectiveEndTime.HasValue)
                {
                    var minutes = (int)Math.Ceiling((effectiveEndTime.Value - nowUtc).TotalMinutes);
                    remainingMinutes = Math.Max(0, minutes);
                }
            }

            result.Add(new MachineStatusDto
            {
                MachineId = machine.MachineId,
                MachineName = machine.MachineName,
                HasActiveSession = hasActiveSession,
                RemainingMinutes = remainingMinutes,
                IsAvailable = hasActiveSession ? false : machine.IsAvailable,
                CurrentUserId = hasActiveSession ? activeSession!.UserId : null
            });
        }

        return result;
    }

    public async Task StartSessionAsync(int machineId, int tenantId, int durationMinutes)
    {
        if (durationMinutes <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero minutes.", nameof(durationMinutes));
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var machine = await _dbContext.Machines
                .FirstOrDefaultAsync(m => m.MachineId == machineId);

            if (machine == null)
            {
                throw new KeyNotFoundException($"Machine with id {machineId} was not found.");
            }

            if (!machine.IsAvailable)
            {
                throw new MachineBusyException("Machine is currently busy.");
            }

            var hasActiveSession = await _dbContext.MachineSessions
                .AnyAsync(s => s.MachineId == machineId && s.Status == MachineSessionStatus.Active);

            if (hasActiveSession)
            {
                throw new MachineBusyException("Machine is currently busy.");
            }

            var startTimeUtc = DateTime.UtcNow;
            var endTimeUtc = startTimeUtc.AddMinutes(durationMinutes);

            var session = new MachineSession
            {
                MachineId = machineId,
                UserId = tenantId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                Status = MachineSessionStatus.Active
            };

            _dbContext.MachineSessions.Add(session);

            machine.IsAvailable = false;
            machine.CurrentUserId = tenantId;
            machine.EndTime = endTimeUtc;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new MachineBusyException("Machine already taken. Please refresh and try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CompleteSessionAsync(int machineId, int tenantId, bool isAdmin)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var activeSession = await _dbContext.MachineSessions
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync(s => s.MachineId == machineId && s.Status == MachineSessionStatus.Active);

            if (activeSession is null)
            {
                await transaction.CommitAsync();
                return;
            }

            if (!isAdmin && activeSession.UserId != tenantId)
            {
                throw new UnauthorizedAccessException("Only the session owner or an admin can complete this session.");
            }

            var machine = await _dbContext.Machines
                .FirstOrDefaultAsync(m => m.MachineId == machineId);

            if (machine is null)
            {
                throw new KeyNotFoundException($"Machine with id {machineId} was not found.");
            }

            activeSession.Status = MachineSessionStatus.Completed;
            activeSession.EndTime = DateTime.UtcNow;

            machine.IsAvailable = true;
            machine.CurrentUserId = null;
            machine.EndTime = null;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
