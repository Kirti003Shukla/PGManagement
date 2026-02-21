import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TenantApprovalApi, TenantProfileUpdateRequest } from '../../core/tenant-approval.api';
import { TenantState } from '../../core/tenant-state';

@Component({
  selector: 'app-tenant-onboarding',
  imports: [CommonModule, FormsModule],
  templateUrl: './tenant-onboarding.html',
  styleUrl: './tenant-onboarding.css',
})
export class TenantOnboarding {
  model: TenantProfileUpdateRequest = {
    fullName: '',
    email: '',
    joinDate: '',
  };

  status = '';

  constructor(
    private api: TenantApprovalApi,
    private tenantState: TenantState,
    private router: Router
  ) {}

  submit() {
    this.status = 'Submitting...';
    this.api.updateMyProfile(this.model).subscribe({
      next: () => {
        this.tenantState.markProfileComplete();
        this.status = 'Profile saved. Redirecting...';
        this.router.navigate(['/tenant']);
      },
      error: (err) => {
        this.status = err?.error?.message || 'Failed to save profile.';
      },
    });
  }
}
