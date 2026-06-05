export interface DescriptionSummary {
  description: string;
  occurrences: number;
  totalAmount: number;
}

export interface CategoryExpense {
  categoryId: string;
  categoryName: string;
  totalAmount: number;
  topDescriptions: DescriptionSummary[];
}

export interface MonthlyCategoryReport {
  year: number;
  month: number;
  topCategories: CategoryExpense[];
}

export interface ExpenseReport {
  months: MonthlyCategoryReport[];
}
