import { Injectable, signal } from '@angular/core';

export interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;
  readonly notifications = signal<Notification[]>([]);

  success(message: string): void {
    this.add(message, 'success');
  }

  error(message: string): void {
    this.add(message, 'error');
  }

  info(message: string): void {
    this.add(message, 'info');
  }

  remove(id: number): void {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }

  private add(message: string, type: Notification['type']): void {
    const id = this.nextId++;
    this.notifications.update(list => [...list, { id, message, type }]);
    setTimeout(() => this.remove(id), 4000);
  }
}
