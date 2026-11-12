import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { Customer } from './customer.model';
import { CustomersComponent } from './customers.component';
import { CustomersService } from './customers.service';

describe('CustomersComponent', () => {
  const existingCustomer: Customer = {
    id: 7,
    name: 'Ada Lovelace',
    email: 'ada@example.com',
    createdAtUtc: '2025-01-01T00:00:00Z'
  };
  const updatedCustomer: Customer = { ...existingCustomer, name: 'Ada Byron' };
  let updateCustomer: jasmine.Spy;

  beforeEach(async () => {
    updateCustomer = jasmine.createSpy('updateCustomer').and.returnValue(of(updatedCustomer));
    await TestBed.configureTestingModule({
      imports: [CustomersComponent],
      providers: [
        provideNoopAnimations(),
        {
          provide: CustomersService,
          useValue: {
            getCustomers: () => of([existingCustomer]),
            createCustomer: () => of(existingCustomer),
            updateCustomer
          }
        }
      ]
    }).compileComponents();
  });

  it('updates the selected customer and exits edit mode', () => {
    const fixture = TestBed.createComponent(CustomersComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.editCustomer(existingCustomer);
    component.customerForm.setValue({ name: 'Ada Byron', email: 'ada@example.com' });
    component.saveCustomer();

    expect(updateCustomer).toHaveBeenCalledWith(7, { name: 'Ada Byron', email: 'ada@example.com' });
    expect(component.customers()[0]).toEqual(updatedCustomer);
    expect(component.editingCustomer()).toBeNull();
  });
});
