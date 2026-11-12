import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { Customer } from './customer.model';
import { CustomersService } from './customers.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.css'
})
export class CustomersComponent implements OnInit {
  private readonly customersService = inject(CustomersService);
  private readonly formBuilder = inject(FormBuilder);
  readonly customers = signal<Customer[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingCustomer = signal<Customer | null>(null);
  readonly customerForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]]
  });

  ngOnInit(): void { this.loadCustomers(); }

  loadCustomers(): void {
    this.loading.set(true);
    this.customersService.getCustomers().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: customers => { this.customers.set(customers); this.errorMessage.set(''); },
      error: () => this.errorMessage.set('Customers could not be loaded. Please try again.')
    });
  }

  addCustomer(): void {
    if (this.customerForm.invalid || this.saving()) { this.customerForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.customersService.createCustomer(this.customerForm.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: customer => {
          this.customers.update(customers => [...customers, customer].sort((a, b) => a.name.localeCompare(b.name)));
          this.customerForm.reset();
          this.errorMessage.set('');
        },
        error: () => this.errorMessage.set('Customer could not be added. Check the details and try again.')
      });
  }

  editCustomer(customer: Customer): void {
    this.editingCustomer.set(customer);
    this.customerForm.setValue({ name: customer.name, email: customer.email });
    this.errorMessage.set('');
  }

  cancelEdit(): void {
    this.editingCustomer.set(null);
    this.customerForm.reset();
    this.errorMessage.set('');
  }

  saveCustomer(): void {
    if (this.customerForm.invalid || this.saving()) { this.customerForm.markAllAsTouched(); return; }
    const customer = this.editingCustomer();
    if (!customer) { this.addCustomer(); return; }

    this.saving.set(true);
    this.customersService.updateCustomer(customer.id, this.customerForm.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: updatedCustomer => {
          this.customers.update(customers => customers.map(item => item.id === updatedCustomer.id ? updatedCustomer : item)
            .sort((a, b) => a.name.localeCompare(b.name)));
          this.cancelEdit();
        },
        error: () => this.errorMessage.set('Customer could not be updated. Check the details and try again.')
      });
  }
}
