import { Component, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { ExpenseReport, MonthlyCategoryReport } from '../../models/report';

@Component({
  selector: 'app-reports',
  imports: [FormsModule, CurrencyPipe],
  templateUrl: './reports.html',
  styleUrl: './reports.scss'
})
export class Reports implements OnInit {
  private readonly reportSvc = inject(ReportService);

  report: ExpenseReport | null = null;
  loading = signal(true);
  startDate: string;
  endDate: string;

  readonly monthNames = [
    '', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ];

  constructor() {
    const now = new Date();
    this.startDate = new Date(now.getFullYear(), now.getMonth() - 5, 1).toISOString().slice(0, 10);
    this.endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().slice(0, 10);
  }

  ngOnInit(): void {
    this.filter();
  }

  filter(): void {
    this.loading.set(true);
    this.reportSvc.getExpensesByCategory(this.startDate, this.endDate).subscribe({
      next: (res) => { this.report = res; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
