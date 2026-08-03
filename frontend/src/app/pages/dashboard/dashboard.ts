import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { CategoryService } from '../../services/category.service';
import { AccountService } from '../../services/account.service';
import { Balance, MonthlyBalance } from '../../models/balance';
import { Category } from '../../models/category';
import { Account } from '../../models/account';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, CurrencyPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private readonly dashboard = inject(DashboardService);
  private readonly categorySvc = inject(CategoryService);
  private readonly accountSvc = inject(AccountService);

  balance: Balance | null = null;
  monthlyBalances: MonthlyBalance[] = [];
  categories: Category[] = [];
  accounts: Account[] = [];
  loading = signal(true);
  startDate: string;
  endDate: string;
  filterCategoryId = '';
  filterAccountId = '';
  filterType = '';

  readonly monthNames = [
    '', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
    'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
  ];

  constructor() {
    const now = new Date();
    this.startDate = new Date(now.getFullYear(), 0, 1).toISOString().slice(0, 10);
    this.endDate = new Date(now.getFullYear(), 11, 31).toISOString().slice(0, 10);
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadAccounts();
    this.filter();
  }

  private loadCategories(): void {
    this.categorySvc.getAll().subscribe(res => this.categories = res);
  }

  private loadAccounts(): void {
    this.accountSvc.getAll().subscribe(res => this.accounts = res);
  }

  filter(): void {
    this.loading.set(true);
    const catId = this.filterCategoryId || undefined;
    const accountId = this.filterAccountId || undefined;
    const type = this.filterType || undefined;

    this.dashboard.getBalance(this.startDate, this.endDate, catId, type, accountId).subscribe({
      next: (res) => { this.balance = res; },
      error: () => {}
    });

    this.dashboard.getMonthlyBalances(this.startDate, this.endDate, catId, type, accountId).subscribe({
      next: (res) => { this.monthlyBalances = res; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
