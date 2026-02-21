import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Role } from './role';
import { TenantState } from './tenant-state';

export const adminGuard: CanActivateFn = () => {
  const role = inject(Role).getRole();
  const router = inject(Router);
  if (role === 'Admin') return true;
  return router.parseUrl('/login');
};

export const tenantApprovedGuard: CanActivateFn = () => {
  const role = inject(Role).getRole();
  const tenantState = inject(TenantState);
  const router = inject(Router);

  if (role !== 'Tenant') return router.parseUrl('/tenant/login');

  const status = tenantState.getStatus();
  if (status === 'PENDING') return router.parseUrl('/tenant/pending');
  if (status === 'APPROVED' && !tenantState.isProfileComplete()) return router.parseUrl('/tenant/onboarding');
  if (status === 'APPROVED' && tenantState.isProfileComplete()) return true;

  return router.parseUrl('/tenant/login');
};

export const tenantOnboardingGuard: CanActivateFn = () => {
  const role = inject(Role).getRole();
  const tenantState = inject(TenantState);
  const router = inject(Router);

  if (role !== 'Tenant') return router.parseUrl('/tenant/login');

  const status = tenantState.getStatus();
  if (status === 'PENDING') return router.parseUrl('/tenant/pending');
  if (status === 'APPROVED' && tenantState.isProfileComplete()) return router.parseUrl('/tenant');
  if (status === 'APPROVED') return true;

  return router.parseUrl('/tenant/login');
};
