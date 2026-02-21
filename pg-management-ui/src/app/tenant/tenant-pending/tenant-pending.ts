import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantState } from '../../core/tenant-state';

@Component({
  selector: 'app-tenant-pending',
  imports: [CommonModule],
  templateUrl: './tenant-pending.html',
  styleUrl: './tenant-pending.css',
})
export class TenantPending {
  constructor(public tenantState: TenantState) {}
}
