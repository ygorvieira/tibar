export interface Account {
  id: string;
  description: string;
  type: 'Checking' | 'Investment' | 'CreditCard';
  createdAt: string;
}

export interface CreateAccountRequest {
  description: string;
  type: 'Checking' | 'Investment' | 'CreditCard';
}

export interface UpdateAccountRequest {
  description: string;
  type: 'Checking' | 'Investment' | 'CreditCard';
}
