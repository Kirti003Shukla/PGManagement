import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MachineApiService, MachineStatus } from '../../core/machine.api';
import { TenantState } from '../../core/tenant-state';

@Component({
  selector: 'app-utilities',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './utilities.component.html',
  styleUrls: ['./utilities.component.css'],
})
export class UtilitiesComponent implements OnInit, OnDestroy {
  machines: MachineStatus[] = [];
  loading = false;
  private pollingTimerId: number | undefined;
  private readonly currentTenantId: number | null;

  constructor(
    private readonly machineApi: MachineApiService,
    private readonly tenantState: TenantState
  ) {
    const tenantId = this.tenantState.getTenantId();
    this.currentTenantId = tenantId ? Number.parseInt(tenantId, 10) : null;
  }

  ngOnInit(): void {
    this.loadMachineStatus();
    this.pollingTimerId = window.setInterval(() => {
      this.loadMachineStatus();
    }, 10_000);
  }

  ngOnDestroy(): void {
    if (this.pollingTimerId !== undefined) {
      window.clearInterval(this.pollingTimerId);
      this.pollingTimerId = undefined;
    }
  }

  start(machineId: number): void {
    const minutesStr = window.prompt(`Enter run time in minutes for machine ${machineId} (e.g. 45):`, '45');
    if (!minutesStr) {
      return;
    }

    const durationMinutes = Number.parseInt(minutesStr, 10);
    if (!Number.isInteger(durationMinutes) || durationMinutes <= 0) {
      return;
    }

    this.machineApi.startMachine(machineId, durationMinutes).subscribe({
      next: () => this.loadMachineStatus(),
      error: () => this.loadMachineStatus(),
    });
  }

  complete(machineId: number): void {
    this.machineApi.completeMachine(machineId).subscribe({
      next: () => this.loadMachineStatus(),
      error: () => this.loadMachineStatus(),
    });
  }

  canStart(machine: MachineStatus): boolean {
    return machine.isAvailable;
  }

  canComplete(machine: MachineStatus): boolean {
    if (machine.isAvailable || this.currentTenantId == null || machine.currentUserId == null) {
      return false;
    }

    return machine.currentUserId === this.currentTenantId;
  }

  isUsedByAnotherTenant(machine: MachineStatus): boolean {
    if (machine.isAvailable) {
      return false;
    }

    return !this.canComplete(machine);
  }

  remainingLabel(machine: MachineStatus): string {
    if (machine.isAvailable || machine.remainingMinutes == null) {
      return '';
    }

    return machine.remainingMinutes <= 0 ? '0 min' : `${machine.remainingMinutes} min`;
  }

  displayMachines(): MachineStatus[] {
    if (this.machines.length >= 4) {
      return this.machines.slice(0, 4);
    }

    const fallback = [...this.machines];
    for (let id = fallback.length + 1; id <= 4; id++) {
      fallback.push({
        machineId: id,
        machineName: `Washing Machine ${id}`,
        hasActiveSession: false,
        remainingMinutes: null,
        isAvailable: true,
        currentUserId: null,
      });
    }

    return fallback;
  }

  formatInUseStatus(machine: MachineStatus): string {
    return machine.isAvailable ? 'Available' : 'Busy';
  }

  private loadMachineStatus(): void {
    this.loading = true;
    this.machineApi.getMachineStatus().subscribe({
      next: (machines) => {
        this.machines = machines;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
