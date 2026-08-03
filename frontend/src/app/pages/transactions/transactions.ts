import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { TransactionService } from '../../services/transaction.service';
import { CategoryService } from '../../services/category.service';
import { AccountService } from '../../services/account.service';
import { NotificationService } from '../../services/notification.service';
import { Transaction, CreateTransactionRequest, UpdateTransactionRequest } from '../../models/transaction';
import { Category } from '../../models/category';
import { Account } from '../../models/account';

@Component({
  selector: 'app-transactions',
  imports: [FormsModule, DatePipe, CurrencyPipe],
  templateUrl: './transactions.html',
  styleUrl: './transactions.scss'
})
export class Transactions implements OnInit {
  private readonly transactionSvc = inject(TransactionService);
  private readonly categorySvc = inject(CategoryService);
  private readonly accountSvc = inject(AccountService);
  private readonly notifications = inject(NotificationService);

  transactions: Transaction[] = [];
  categories: Category[] = [];
  accounts: Account[] = [];
  startDate: string;
  endDate: string;
  filterCategoryId = '';
  filterAccountId = '';
  filterType = '';
  showForm = false;
  editingId: string | null = null;
  loading = signal(true);
  saving = signal(false);

  form: CreateTransactionRequest = {
    description: '',
    amount: 0,
    type: 'Expense',
    categoryId: '',
    accountId: '',
    date: ''
  };

  constructor() {
    const now = new Date();
    this.startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10);
    this.endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().slice(0, 10);
  }

  ngOnInit(): void {
    this.loadTransactions();
    this.loadCategories();
    this.loadAccounts();
  }

  private loadTransactions(): void {
    this.loading.set(true);
    this.transactionSvc.getByPeriod(
      this.startDate,
      this.endDate,
      this.filterCategoryId || undefined,
      this.filterType || undefined,
      this.filterAccountId || undefined
    ).subscribe({
      next: (res) => { this.transactions = res.items; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  private loadCategories(): void {
    this.categorySvc.getAll().subscribe(res => this.categories = res);
  }

  private loadAccounts(): void {
    this.accountSvc.getAll().subscribe(res => this.accounts = res);
  }

  filter(): void {
    this.loadTransactions();
  }

  openCreate(): void {
    this.editingId = null;
    this.form = { description: '', amount: 0, type: 'Expense', categoryId: '', accountId: '', date: '' };
    this.showForm = true;
  }

  openEdit(t: Transaction): void {
    this.editingId = t.id;
    this.form = {
      description: t.description,
      amount: t.amount,
      type: t.type,
      categoryId: t.categoryId,
      accountId: t.accountId,
      date: t.date
    };
    this.showForm = true;
  }

  cancelForm(): void {
    if (this.formHasData() && !confirm('Cancelar a operação? Os dados preenchidos serão perdidos.'))
      return;
    this.resetForm();
  }

  private resetForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  private formHasData(): boolean {
    return this.form.description.trim() !== ''
      || this.form.amount !== 0
      || this.form.categoryId !== ''
      || this.form.accountId !== ''
      || this.form.date !== '';
  }

  save(): void {
    this.saving.set(true);

    if (this.editingId) {
      const data: UpdateTransactionRequest = {
        description: this.form.description,
        amount: this.form.amount,
        categoryId: this.form.categoryId,
        accountId: this.form.accountId,
        date: this.form.date
      };
      this.transactionSvc.update(this.editingId, data).subscribe({
        next: () => {
          this.notifications.success('Transação atualizada.');
          this.resetForm(); this.loadTransactions(); this.saving.set(false);
        },
        error: () => this.saving.set(false)
      });
    } else {
      this.transactionSvc.create(this.form).subscribe({
        next: () => {
          this.notifications.success('Transação criada.');
          this.resetForm(); this.loadTransactions(); this.saving.set(false);
        },
        error: () => this.saving.set(false)
      });
    }
  }

  delete(id: string): void {
    if (!confirm('Excluir esta transação?')) return;
    this.transactionSvc.delete(id).subscribe(() => {
      this.notifications.success('Transação excluída.');
      this.loadTransactions();
    });
  }
}
