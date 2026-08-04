import { TestBed } from '@angular/core/testing';
import { Transactions } from './transactions';
import { TransactionService } from '../../services/transaction.service';
import { CategoryService } from '../../services/category.service';
import { AccountService } from '../../services/account.service';
import { NotificationService } from '../../services/notification.service';
import { of } from 'rxjs';

describe('Transactions', () => {
  let component: Transactions;
  const createSpy = vi.fn(() => of([]));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Transactions],
      providers: [
        {
          provide: TransactionService,
          useValue: { getByPeriod: () => of({ items: [] }), create: createSpy },
        },
        { provide: CategoryService, useValue: { getAll: () => of([]) } },
        { provide: AccountService, useValue: { getAll: () => of([]) } },
        { provide: NotificationService, useValue: { success: () => {} } },
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    const fixture = TestBed.createComponent(Transactions);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should close form immediately when no data is filled', () => {
    component.showForm = true;
    component.form = { description: '', amount: 0, type: 'Expense', categoryId: '', accountId: '', date: '' };

    component.cancelForm();

    expect(component.showForm).toBe(false);
    expect(component.editingId).toBeNull();
  });

  it('should prompt confirm when form has description', () => {
    component.showForm = true;
    component.form.description = 'Test';
    const spy = vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.cancelForm();

    expect(spy).toHaveBeenCalledWith('Cancelar a operação? Os dados preenchidos serão perdidos.');
    expect(component.showForm).toBe(true);
  });

  it('should prompt confirm when form has amount', () => {
    component.showForm = true;
    component.form.amount = 100;
    const spy = vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.cancelForm();

    expect(spy).toHaveBeenCalled();
    expect(component.showForm).toBe(true);
  });

  it('should prompt confirm when form has category', () => {
    component.showForm = true;
    component.form.categoryId = 'some-id';
    const spy = vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.cancelForm();

    expect(spy).toHaveBeenCalled();
    expect(component.showForm).toBe(true);
  });

  it('should prompt confirm when form has date', () => {
    component.showForm = true;
    component.form.date = '2026-05-29';
    const spy = vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.cancelForm();

    expect(spy).toHaveBeenCalled();
    expect(component.showForm).toBe(true);
  });

  it('should close form when user confirms', () => {
    component.showForm = true;
    component.form.description = 'Test';
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.cancelForm();

    expect(component.showForm).toBe(false);
    expect(component.editingId).toBeNull();
  });

  it('should prompt confirm when installment mode is on', () => {
    component.showForm = true;
    component.isInstallment = true;
    const spy = vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.cancelForm();

    expect(spy).toHaveBeenCalled();
    expect(component.showForm).toBe(true);
  });

  it('should send installments count on create when installment mode is on', () => {
    createSpy.mockClear();
    component.openCreate();
    component.form = {
      description: 'Laptop', amount: 100, type: 'Expense',
      categoryId: 'cat', accountId: 'acc', date: '2026-01-01',
    };
    component.isInstallment = true;
    component.installments = 5;

    component.save();

    expect(createSpy).toHaveBeenCalledWith(expect.objectContaining({ installments: 5 }));
  });

  it('should not send installments when installment mode is off', () => {
    createSpy.mockClear();
    component.openCreate();
    component.form = {
      description: 'Lunch', amount: 20, type: 'Expense',
      categoryId: 'cat', accountId: 'acc', date: '2026-01-01',
    };
    component.isInstallment = false;

    component.save();

    expect(createSpy).toHaveBeenCalledWith(expect.not.objectContaining({ installments: expect.anything() }));
  });
});
