export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Transaction {
  id: string;
  description: string;
  amount: number;
  currency: string;
  type: 'Income' | 'Expense';
  categoryId: string;
  categoryName: string;
  accountId: string;
  accountName: string;
  date: string;
  installmentId: string | null;
  createdAt: string;
}

export interface CreateTransactionRequest {
  description: string;
  amount: number;
  type: 'Income' | 'Expense';
  categoryId: string;
  accountId: string;
  date: string;
  installments?: number;
}

export interface UpdateTransactionRequest {
  description: string;
  amount: number;
  categoryId: string;
  accountId: string;
  date: string;
}
