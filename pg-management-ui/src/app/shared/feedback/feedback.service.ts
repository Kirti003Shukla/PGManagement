import { Injectable } from '@angular/core';

export interface FeedbackItem {
  id: string;
  name?: string;
  message: string;
  fileName?: string;
  fileDataUrl?: string;
  timestamp: number;
}

const STORAGE_KEY = 'pg_feedback_items_v1';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private items: FeedbackItem[] = [];

  private load(): FeedbackItem[] {
    if (this.items && this.items.length) return this.items;
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      this.items = raw ? JSON.parse(raw) : [];
    } catch (e) {
      this.items = [];
    }
    return this.items;
  }

  private saveStore() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.items || []));
    } catch (e) {
      console.warn('Failed to persist feedback', e);
    }
  }

  async submit(feedback: Omit<FeedbackItem, 'id' | 'timestamp'>) {
    const items = this.load();
    const item: FeedbackItem = {
      id: Math.random().toString(36).slice(2, 9),
      timestamp: Date.now(),
      ...feedback,
    };
    items.unshift(item);
    this.saveStore();
    return item;
  }

  list(): FeedbackItem[] {
    return this.load().slice();
  }

  remove(id: string) {
    const items = this.load();
    const idx = items.findIndex(i => i.id === id);
    if (idx >= 0) {
      items.splice(idx, 1);
      this.saveStore();
      return true;
    }
    return false;
  }
}
