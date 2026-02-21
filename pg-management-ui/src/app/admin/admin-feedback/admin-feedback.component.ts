import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FeedbackService, FeedbackItem } from '../../shared/feedback/feedback.service';

@Component({
  selector: 'app-admin-feedback',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-feedback.component.html',
  styleUrls: ['./admin-feedback.component.css'],
})
export class AdminFeedbackComponent {
  items: FeedbackItem[] = [];

  constructor(private svc: FeedbackService) {
    this.load();
  }

  load() {
    this.items = this.svc.list();
  }

  remove(id: string) {
    if (!confirm('Remove this feedback?')) return;
    this.svc.remove(id);
    this.load();
  }
}
