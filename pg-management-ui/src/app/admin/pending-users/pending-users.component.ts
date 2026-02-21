import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantApprovalApi, PendingTenant } from '../../core/tenant-approval.api';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-pending-users',
  imports: [CommonModule],
  templateUrl: './pending-users.component.html',
  styleUrls: ['./pending-users.component.css'],
})
export class PendingUsersComponent {
  loading = false;
  error = '';
  items: PendingTenant[] = [];

  constructor(private api: TenantApprovalApi, private cdr: ChangeDetectorRef) {
    this.refresh();
  }

  refresh() {
    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();
    this.api.listPendingTenants().subscribe({
      next: (rows) => {
        this.items = rows || [];
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err?.error?.message || err?.message || 'Failed to load pending users.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  approve(item: PendingTenant) {
    this.api.approveTenant(item.tenantId).subscribe({
      next: () => this.refresh(),
      error: (err) => {
        this.error = err?.error?.message || err?.message || 'Failed to approve user.';
        this.cdr.markForCheck();
      },
    });
  }
}
