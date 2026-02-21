import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MachineStatus {
  machineId: number;
  machineName: string;
  hasActiveSession: boolean;
  remainingMinutes: number | null;
  isAvailable: boolean;
  currentUserId: number | null;
}

export interface StartMachineRequest {
  durationMinutes: number;
}

@Injectable({ providedIn: 'root' })
export class MachineApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getMachineStatus(): Observable<MachineStatus[]> {
    return this.http.get<MachineStatus[]>(`${this.apiUrl}/machines/status`);
  }

  startMachine(machineId: number, durationMinutes: number): Observable<void> {
    const payload: StartMachineRequest = { durationMinutes };
    return this.http.post<void>(`${this.apiUrl}/machines/${encodeURIComponent(String(machineId))}/start`, payload);
  }

  completeMachine(machineId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/machines/${encodeURIComponent(String(machineId))}/complete`, {});
  }
}
