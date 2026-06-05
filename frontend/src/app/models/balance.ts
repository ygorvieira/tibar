export interface Balance {
  totalIncome: number;
  totalExpense: number;
  balance: number;
  currency: string;
  startDate: string;
  endDate: string;
}

export interface MonthlyBalance {
  year: number;
  month: number;
  totalIncome: number;
  totalExpense: number;
  balance: number;
  currency: string;
}
