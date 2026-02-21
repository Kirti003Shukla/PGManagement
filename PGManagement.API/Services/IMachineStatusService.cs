using PGManagement.API.DTOs;

namespace PGManagement.API.Services;

public interface IMachineStatusService
{
    Task<List<MachineStatusDto>> GetMachineStatusesAsync();
    Task StartSessionAsync(int machineId, int tenantId, int durationMinutes);
    Task CompleteSessionAsync(int machineId, int tenantId, bool isAdmin);
}
