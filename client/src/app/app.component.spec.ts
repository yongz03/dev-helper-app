import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { CustomersService } from './customers/customers.service';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideNoopAnimations(),
        { provide: CustomersService, useValue: { getCustomers: () => of([]), createCustomer: () => of({}) } }
      ]
    }).compileComponents();
  });

  it('renders the customers list and add form', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Customers');
    expect(compiled.querySelector('button')?.textContent).toContain('Add customer');
  });
});
