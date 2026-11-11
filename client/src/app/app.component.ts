import { Component } from '@angular/core';
import { CustomersComponent } from './customers/customers.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CustomersComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'client';
}
