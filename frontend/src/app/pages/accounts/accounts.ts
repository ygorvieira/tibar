import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { AccountService } from '../../services/account.service';
import { NotificationService } from '../../services/notification.service';
import { Account, CreateAccountRequest, UpdateAccountRequest } from '../../models/account';

@Component({
  selector: 'app-accounts',
  imports: [FormsModule, DatePipe],
  templateUrl: './accounts.html',
  styleUrl: './accounts.scss'
})
export class Accounts implements OnInit {
  private readonly accountSvc = inject(AccountService);
  private readonly notifications = inject(NotificationService);

  accounts: Account[] = [];
  showForm = false;
  editingId: string | null = null;
  loading = signal(true);

  form: CreateAccountRequest = { description: '', type: 'Checking' };

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.accountSvc.getAll().subscribe({
      next: (res) => { this.accounts = res; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId = null;
    this.form = { description: '', type: 'Checking' };
    this.showForm = true;
  }

  openEdit(a: Account): void {
    this.editingId = a.id;
    this.form = { description: a.description, type: a.type };
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  save(): void {
    if (this.editingId) {
      const data: UpdateAccountRequest = { description: this.form.description, type: this.form.type };
      this.accountSvc.update(this.editingId, data).subscribe({
        next: () => {
          this.notifications.success('Conta atualizada.');
          this.cancelForm(); this.load();
        }
      });
    } else {
      this.accountSvc.create(this.form).subscribe({
        next: () => {
          this.notifications.success('Conta criada.');
          this.cancelForm(); this.load();
        }
      });
    }
  }

  delete(id: string): void {
    if (!confirm('Excluir esta conta?')) return;
    this.accountSvc.delete(id).subscribe({
      next: () => {
        this.notifications.success('Conta excluída.');
        this.load();
      },
      error: () => this.load()
    });
  }

  typeLabel(type: string): string {
    switch (type) {
      case 'Investment': return 'Investimento';
      case 'CreditCard': return 'Cartão de Crédito';
      default: return 'Conta Corrente';
    }
  }
}
