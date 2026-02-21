import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FeedbackService } from './feedback.service';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.css'],
})
export class FeedbackComponent {
  message = '';
  name = '';
  file?: File | null = null;
  previewDataUrl?: string;
  submitting = false;
  success = '';

  constructor(private svc: FeedbackService) {}

  onFileChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const f = input.files && input.files[0];
    if (!f) {
      this.file = null;
      this.previewDataUrl = undefined;
      return;
    }
    this.file = f;
    const reader = new FileReader();
    reader.onload = () => { this.previewDataUrl = reader.result as string; };
    reader.readAsDataURL(f);
  }

  async submit() {
    if (!this.message.trim() && !this.previewDataUrl) {
      this.success = 'Please add a message or attach a screenshot.';
      return;
    }
    this.submitting = true;
    try {
      await this.svc.submit({
        name: this.name.trim() || undefined,
        message: this.message.trim() || '(no message)',
        fileName: this.file ? this.file.name : undefined,
        fileDataUrl: this.previewDataUrl,
      });
      this.success = 'Thanks — feedback saved.';
      this.message = '';
      this.name = '';
      this.file = null;
      this.previewDataUrl = undefined;
      const input = document.getElementById('fb-file') as HTMLInputElement | null;
      if (input) input.value = '';
    } finally {
      this.submitting = false;
      setTimeout(() => (this.success = ''), 4000);
    }
  }
}
