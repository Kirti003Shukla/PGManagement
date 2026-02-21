import { Injectable } from '@angular/core';
import { TenantApprovalStatus } from './tenant-approval.api';

const TENANT_ID_KEY = 'tenantId';
const TENANT_STATUS_KEY = 'tenantStatus';
const TENANT_PROFILE_COMPLETE_KEY = 'tenantProfileComplete';

@Injectable({ providedIn: 'root' })
export class TenantState {
  getTenantId(): string | null {
    return localStorage.getItem(TENANT_ID_KEY);
  }

  getStatus(): TenantApprovalStatus | null {
    return localStorage.getItem(TENANT_STATUS_KEY) as any;
  }

  isProfileComplete(): boolean {
    return localStorage.getItem(TENANT_PROFILE_COMPLETE_KEY) === 'true';
  }

  saveTenantSession(payload: { tenantId: number; status: TenantApprovalStatus; profileComplete: boolean }) {
    localStorage.setItem(TENANT_ID_KEY, String(payload.tenantId));
    localStorage.setItem(TENANT_STATUS_KEY, payload.status);
    localStorage.setItem(TENANT_PROFILE_COMPLETE_KEY, String(payload.profileComplete));
  }

  markProfileComplete() {
    localStorage.setItem(TENANT_PROFILE_COMPLETE_KEY, 'true');
  }

  clear() {
    localStorage.removeItem(TENANT_ID_KEY);
    localStorage.removeItem(TENANT_STATUS_KEY);
    localStorage.removeItem(TENANT_PROFILE_COMPLETE_KEY);
  }
}
