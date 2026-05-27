import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { Balance } from '../../models/balance';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, CurrencyPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private readonly dashboard = inject(DashboardService);

  balance: Balance | null = null;
  loading = signal(true);
  startDate: string;
  endDate: string;

  constructor() {
    const now = new Date();
    this.startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10);
    this.endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().slice(0, 10);
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.dashboard.getBalance(this.startDate, this.endDate).subscribe({
      next: (res) => { this.balance = res; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
