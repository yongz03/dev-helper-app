export interface Customer {
  id: number;
  name: string;
  email: string;
  createdAtUtc: string;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
}

export type UpdateCustomerRequest = CreateCustomerRequest;
