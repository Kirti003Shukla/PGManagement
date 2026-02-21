import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminFeedbackComponent } from '../admin-feedback/admin-feedback.component';


@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, AdminFeedbackComponent],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.css'],
})
export class AdminDashboard {

}
