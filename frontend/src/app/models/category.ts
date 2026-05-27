export interface Category {
  id: string;
  name: string;
  type: 'Income' | 'Expense';
  createdAt: string;
}

export interface CreateCategoryRequest {
  name: string;
  type: 'Income' | 'Expense';
}

export interface UpdateCategoryRequest {
  name: string;
  type: 'Income' | 'Expense';
}
