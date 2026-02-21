import { Routes } from '@angular/router';
import { AdminDashboard } from './admin/admin-dashboard/admin-dashboard';
import { TenantDashboard } from './tenant/tenant-dashboard/tenant-dashboard';
import { FeedbackComponent } from './shared/feedback/feedback.component';
import { AdminFeedbackComponent } from './admin/admin-feedback/admin-feedback.component';
import { Layout } from './shared/layout/layout';
import { Login } from './auth/login/login';
import { PhoneAuthComponent } from './auth/phone-auth/phone-auth.component';
import { PendingUsersComponent } from './admin/pending-users/pending-users.component';
import { TenantPending } from './tenant/tenant-pending/tenant-pending';
import { TenantOnboarding } from './tenant/tenant-onboarding/tenant-onboarding';
import { adminGuard, tenantApprovedGuard, tenantOnboardingGuard } from './core/route-guards';

export const routes: Routes = [ 
 {   path: 'login', component: Login },
{   path: 'tenant/login', component: PhoneAuthComponent },
{   path: 'phone-auth', component: PhoneAuthComponent },
 {   path: '',      component: Layout, 
     children: [ 
  { path: 'admin', component: AdminDashboard },
    { path: 'admin/pending-users', component: PendingUsersComponent, canActivate: [adminGuard] },
    { path: 'tenant', component: TenantDashboard, canActivate: [tenantApprovedGuard] },
   { path: 'tenant/pending', component: TenantPending },
    { path: 'tenant/onboarding', component: TenantOnboarding, canActivate: [tenantOnboardingGuard] },
    { path: 'feedback', component: FeedbackComponent }
 ]
},
{   path: 'admin/feedback', component: AdminFeedbackComponent },
{    path: '', redirectTo: 'tenant', pathMatch: 'full' }
];
