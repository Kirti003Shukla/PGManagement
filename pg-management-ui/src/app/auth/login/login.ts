import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Auth, LoginResponse } from '../../core/auth';

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {

   constructor(
    private auth: Auth,
    private router: Router
  ) {}

  loginAsAdmin() {
    const email = 'admin@pg.com';
    const password = '1234';

    this.auth.login(email, password).subscribe((response: LoginResponse) => {
      this.auth.saveRole(response.role);
      this.auth.saveAdminBasicAuth(email, password);
      this.router.navigate(['/admin']);
    });
  }


  loginAsTenant() {
    this.router.navigate(['/phone-auth']);
  }
}
  
