import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { from, of, switchMap, throwError } from 'rxjs';
import { firebaseAuth } from '../shared/firebase/firebase';
import { Auth } from './auth';

export type TenantApprovalStatus = 'PENDING' | 'APPROVED' | 'REJECTED';

export interface TenantLoginResponse {
  tenantId: number;
  phoneNumber: string;
  role: string;
  isApproved: boolean;
  profileComplete: boolean;
  message: string;
}

export interface PendingTenant {
  tenantId: number;
  phoneNumber: string;
  createdAtUtc: string;
}

export interface TenantProfileUpdateRequest {
  fullName: string;
  email: string;
  joinDate: string;
}

@Injectable({ providedIn: 'root' })
export class TenantApprovalApi {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private auth: Auth) {}

  private adminHeaders() {
    const header = this.auth.getAdminBasicAuthHeader();
    if (!header) {
      return throwError(() => new Error('Admin is not logged in.'));
    }

    return of({
      headers: new HttpHeaders({
        Authorization: header,
        'X-Admin-Authorization': header,
      }),
    });
  }

  private authHeaders(explicitIdToken?: string) {
    if (explicitIdToken) {
      return of({
        headers: new HttpHeaders({
          Authorization: `Bearer ${explicitIdToken}`,
        }),
      });
    }

    return from(firebaseAuth.currentUser?.getIdToken() ?? Promise.reject(new Error('Not signed in'))).pipe(
      switchMap((token) =>
        of({
          headers: new HttpHeaders({
            Authorization: `Bearer ${token}`,
          }),
        })
      )
    );
  }

  /**
   * Called after OTP verification. Creates/updates a tenant record and returns approval/profile state.
   * - POST {apiUrl}/auth/tenant-login (Firebase Bearer token)
   */
  tenantLogin(explicitIdToken?: string) {
    return this.authHeaders(explicitIdToken).pipe(
      switchMap((opts) => this.http.post<TenantLoginResponse>(`${this.apiUrl}/auth/tenant-login`, {}, opts))
    );
  }

  /**
   * Admin: list pending tenant requests.
   * - GET {apiUrl}/admin/tenants/pending
   */
  listPendingTenants() {
    return this.adminHeaders().pipe(
      switchMap((opts) => this.http.get<PendingTenant[]>(`${this.apiUrl}/admin/tenants/pending`, opts))
    );
  }

  /**
   * Admin: approve a tenant request.
   * - POST {apiUrl}/admin/tenants/{tenantId}/approve
   */
  approveTenant(tenantId: number) {
    return this.adminHeaders().pipe(
      switchMap((opts) =>
        this.http.post<void>(`${this.apiUrl}/admin/tenants/${encodeURIComponent(String(tenantId))}/approve`, {}, opts)
      )
    );
  }

  /**
   * Tenant: submit onboarding/profile details.
   * - PUT {apiUrl}/tenants/me (Firebase Bearer token)
   */
  updateMyProfile(payload: TenantProfileUpdateRequest) {
    return this.authHeaders().pipe(
      switchMap((opts) => this.http.put<void>(`${this.apiUrl}/tenants/me`, payload, opts))
    );
  }
}
