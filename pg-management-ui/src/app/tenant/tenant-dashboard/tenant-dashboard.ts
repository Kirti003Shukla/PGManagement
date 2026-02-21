import { Component } from '@angular/core';
import { UtilitiesComponent } from '../../shared/utilities/utilities.component';
import { FeedbackComponent } from '../../shared/feedback/feedback.component';

@Component({
  selector: 'app-tenant-dashboard',
  imports: [UtilitiesComponent, FeedbackComponent],
  templateUrl: './tenant-dashboard.html',
  styleUrl: './tenant-dashboard.css',
})
export class TenantDashboard {

}
