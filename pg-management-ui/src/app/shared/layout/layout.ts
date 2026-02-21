import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { UtilitiesComponent } from '../utilities/utilities.component';
import { Role } from '../../core/role';
import { Router } from '@angular/router';
import { Auth } from '../../core/auth';


@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterModule, CommonModule, UtilitiesComponent],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {
  constructor(public roleService: Role,
              private router: Router,
              private auth: Auth
  ) {}


  logout() {    
    const role = this.roleService.getRole();
    this.auth.logout();
    this.router.navigate([role === 'Tenant' ? '/tenant/login' : '/login']);
  }
}
