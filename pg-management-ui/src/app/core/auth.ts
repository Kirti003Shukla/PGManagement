import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { signOut } from 'firebase/auth';
import { firebaseAuth } from '../shared/firebase/firebase';

export interface LoginResponse {
  role: string;
}

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private apiUrl = environment.apiUrl;
  private readonly adminBasicAuthKey = 'adminBasicAuth';
  constructor(private http: HttpClient) {}


  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { email, password });
  }

  saveAdminBasicAuth(email: string, password: string) {
    const raw = `${email}:${password}`;
    const token = typeof globalThis.btoa === 'function'
      ? globalThis.btoa(raw)
      : Buffer.from(raw, 'utf-8').toString('base64');
    localStorage.setItem(this.adminBasicAuthKey, token);
  }

  getAdminBasicAuthHeader(): string | null {
    const token = localStorage.getItem(this.adminBasicAuthKey);
    return token ? `Basic ${token}` : null;
  }

  logout() {
    // Clear local app state (role, tenant session, etc.)
    localStorage.clear();

    // Also sign out of Firebase so the phone auth session is terminated.
    // Intentionally fire-and-forget since callers treat logout as sync.
    signOut(firebaseAuth).catch(() => undefined);
  }

  getRole(): string | null {
    return localStorage.getItem('role') as any;
  }

  saveRole(role: string) {
    localStorage.setItem('role', role);
  }

  isLoggedIn(): boolean {
    return !!this.getRole();
  }
}
