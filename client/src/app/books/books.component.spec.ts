import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { Book } from './book.model';
import { BooksComponent } from './books.component';
import { BooksService } from './books.service';

describe('BooksComponent', () => {
  const existingBook: Book = { id: 7, name: 'Dune', fileType: 'PDF', filePath: 'C:\\Books\\dune.pdf' };
  const updatedBook: Book = { ...existingBook, name: 'Dune Messiah' };
  let updateBook: jasmine.Spy;
  let createBook: jasmine.Spy;

  beforeEach(async () => {
    updateBook = jasmine.createSpy('updateBook').and.returnValue(of(updatedBook));
    createBook = jasmine.createSpy('createBook').and.returnValue(of(existingBook));
    await TestBed.configureTestingModule({
      imports: [BooksComponent],
      providers: [provideNoopAnimations(), { provide: BooksService, useValue: {
        getBooks: () => of([existingBook]), createBook, importBooks: () => of({ importedCount: 0, skippedCount: 0 }), openBook: () => of(undefined), updateBook
      } }]
    }).compileComponents();
  });

  it('updates the selected book and exits edit mode', () => {
    const fixture = TestBed.createComponent(BooksComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.editBook(existingBook);
    component.bookForm.setValue({ name: 'Dune Messiah', subfolder: 'Sci-Fi', fileType: 'EPUB' });
    component.saveBook();
    expect(updateBook).toHaveBeenCalledWith(7, {
      name: 'Dune Messiah', fileType: 'EPUB', filePath: 'D:\\Users\\Yong Zhang\\Documents\\eBooks\\Sci-Fi\\Dune Messiah.epub'
    });
    expect(component.books()[0]).toEqual(updatedBook);
    expect(component.editingBook()).toBeNull();
  });

  it('builds a book path from the default folder, subfolder, and selected type', () => {
    const fixture = TestBed.createComponent(BooksComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.bookForm.setValue({ name: 'Dune', subfolder: 'Science Fiction', fileType: 'MOBI' });

    component.addBook();

    expect(createBook).toHaveBeenCalledWith({
      name: 'Dune', fileType: 'MOBI', filePath: 'D:\\Users\\Yong Zhang\\Documents\\eBooks\\Science Fiction\\Dune.mobi'
    });
  });

  it('opens the selected book through the local API', () => {
    const fixture = TestBed.createComponent(BooksComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const openBook = spyOn(TestBed.inject(BooksService), 'openBook').and.returnValue(of(undefined));

    component.openBook(existingBook);

    expect(openBook).toHaveBeenCalledWith(existingBook.id);
    expect(component.successMessage()).toBe('Dune opened successfully.');
  });

  it('imports books from the entered folder and refreshes the list', () => {
    const fixture = TestBed.createComponent(BooksComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const importBooks = spyOn(TestBed.inject(BooksService), 'importBooks').and.returnValue(of({ importedCount: 2, skippedCount: 1 }));
    component.importFolder.setValue('D:\\Books');

    component.importBooks();

    expect(importBooks).toHaveBeenCalledWith('D:\\Books');
    expect(component.importMessage()).toBe('Imported 2 books; skipped 1 already in the library.');
  });
});
