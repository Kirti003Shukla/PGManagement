import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class Role {

  getRole(): 'Admin' | 'Tenant' | null {
    return localStorage.getItem('role') as any;
  }

}
