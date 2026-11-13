import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { BooksService } from './books/books.service';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideNoopAnimations(), { provide: BooksService, useValue: { getBooks: () => of([]), createBook: () => of({}) } }]
    }).compileComponents();
  });

  it('renders the books list and add form', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Books');
    expect(compiled.querySelector('.form-actions button[type="submit"]')?.textContent).toContain('Add book');
  });
});
