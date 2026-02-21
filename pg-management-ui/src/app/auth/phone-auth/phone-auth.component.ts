import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RecaptchaVerifier, signInWithPhoneNumber } from 'firebase/auth';
import { firebaseAuth } from '../../shared/firebase/firebase';
import { Auth as AuthService } from '../../core/auth';
import { TenantApprovalApi } from '../../core/tenant-approval.api';
import { TenantState } from '../../core/tenant-state';

@Component({
  selector: 'app-phone-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './phone-auth.component.html',
  styleUrls: ['./phone-auth.component.css'],
})
export class PhoneAuthComponent {
  phone = '';
  code = '';
  confirmationResult: any = null;
  status = '';

  @ViewChild('recaptcha') recaptcha?: ElementRef<HTMLDivElement>;

  constructor(
    private router: Router,
    private authService: AuthService,
    private tenantApprovalApi: TenantApprovalApi,
    private tenantState: TenantState
  ) {}

  async sendOtp() {
    this.status = '';
    try {
      const container = this.recaptcha?.nativeElement;
      if (!container) throw new Error('reCAPTCHA container not found — ensure #recaptcha is present in template and not hidden by *ngIf');

      const verifier = new RecaptchaVerifier(firebaseAuth, container, { size: 'invisible' });
      this.status = 'Sending code...';
      const result = await signInWithPhoneNumber(firebaseAuth, this.phone, verifier);
      this.confirmationResult = result;
      this.status = 'Code sent. Enter the code you received.';
    } catch (e: any) {
      this.status = 'Failed to send code: ' + (e.message || e);
    }
  }

  async verifyCode() {
    if (!this.confirmationResult) {
      this.status = 'No code request active.';
      return;
    }

    try {
      this.status = 'Verifying...';
      const userCredential = await this.confirmationResult.confirm(this.code);
      const idToken = await userCredential.user.getIdToken();

      // Persist tenant phone number in DB via backend; no phone/token stored in localStorage.
      this.status = 'Phone verified. Checking approval status...';
      this.tenantApprovalApi.tenantLogin(idToken).subscribe({
        next: (response) => {
          const status = response.isApproved ? 'APPROVED' : 'PENDING';

          this.tenantState.saveTenantSession({
            tenantId: response.tenantId,
            status,
            profileComplete: response.profileComplete,
          });

          this.authService.saveRole('Tenant');

          if (status === 'PENDING') {
            this.status = 'Awaiting admin approval.';
            this.router.navigate(['/tenant/pending']);
            return;
          }

          if (status === 'APPROVED' && !response.profileComplete) {
            this.status = 'Approved. Please complete your profile.';
            this.router.navigate(['/tenant/onboarding']);
            return;
          }

          if (status === 'APPROVED' && response.profileComplete) {
            this.status = 'Welcome! Redirecting...';
            this.router.navigate(['/tenant']);
            return;
          }

          this.status = response.message || 'Access denied.';
        },
        error: (err) => {
          this.status = err?.error?.message || 'Could not validate tenant status with server.';
        },
      });
    } catch (e: any) {
      this.status = 'Verification failed: ' + (e.message || e);
    }
  }
}
