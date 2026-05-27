import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../models/category';

@Component({
  selector: 'app-categories',
  imports: [FormsModule, DatePipe],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {
  private readonly categorySvc = inject(CategoryService);
  private readonly notifications = inject(NotificationService);

  categories: Category[] = [];
  showForm = false;
  editingId: string | null = null;
  loading = signal(true);

  form: CreateCategoryRequest = { name: '', type: 'Expense' };

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.categorySvc.getAll().subscribe({
      next: (res) => { this.categories = res; this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId = null;
    this.form = { name: '', type: 'Expense' };
    this.showForm = true;
  }

  openEdit(c: Category): void {
    this.editingId = c.id;
    this.form = { name: c.name, type: c.type };
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  save(): void {
    if (this.editingId) {
      const data: UpdateCategoryRequest = { name: this.form.name, type: this.form.type };
      this.categorySvc.update(this.editingId, data).subscribe({
        next: () => {
          this.notifications.success('Categoria atualizada.');
          this.cancelForm(); this.load();
        }
      });
    } else {
      this.categorySvc.create(this.form).subscribe({
        next: () => {
          this.notifications.success('Categoria criada.');
          this.cancelForm(); this.load();
        }
      });
    }
  }

  delete(id: string): void {
    if (!confirm('Excluir esta categoria?')) return;
    this.categorySvc.delete(id).subscribe(() => {
      this.notifications.success('Categoria excluída.');
      this.load();
    });
  }
}
