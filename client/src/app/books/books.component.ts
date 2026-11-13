import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Book } from './book.model';
import { BooksService } from './books.service';

@Component({
  selector: 'app-books', standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, MatSelectModule],
  templateUrl: './books.component.html', styleUrl: './books.component.css'
})
export class BooksComponent implements OnInit {
  private readonly libraryRoot = 'D:\\Users\\Yong Zhang\\Documents\\eBooks';
  private readonly booksService = inject(BooksService);
  private readonly formBuilder = inject(FormBuilder);
  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly openingBookId = signal<number | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly importMessage = signal('');
  readonly importing = signal(false);
  readonly editingBook = signal<Book | null>(null);
  readonly importFolder = this.formBuilder.nonNullable.control('D:\\Users\\Yong Zhang\\Documents\\eBooks');
  readonly bookForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    subfolder: [''],
    fileType: ['PDF', [Validators.required]]
  });

  ngOnInit(): void { this.loadBooks(); }

  loadBooks(): void {
    this.loading.set(true);
    this.booksService.getBooks().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: books => { this.books.set(books); this.errorMessage.set(''); },
      error: () => this.errorMessage.set('Books could not be loaded. Please try again.')
    });
  }

  importBooks(): void {
    const folderPath = this.importFolder.value.trim();
    if (!folderPath || this.importing()) return;
    this.importing.set(true);
    this.importMessage.set('');
    this.errorMessage.set('');
    this.booksService.importBooks(folderPath).pipe(finalize(() => this.importing.set(false))).subscribe({
      next: result => {
        this.importMessage.set(`Imported ${result.importedCount} book${result.importedCount === 1 ? '' : 's'}; skipped ${result.skippedCount} already in the library.`);
        this.loadBooks();
      },
      error: error => this.importMessage.set(this.importError(error))
    });
  }

  openBook(book: Book): void {
    if (this.openingBookId() !== null) return;
    this.openingBookId.set(book.id);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.booksService.openBook(book.id).pipe(finalize(() => this.openingBookId.set(null))).subscribe({
      next: () => this.successMessage.set(`${book.name} opened successfully.`),
      error: error => this.errorMessage.set(this.openBookError(error, book))
    });
  }

  addBook(): void {
    if (this.bookForm.invalid || this.saving()) { this.bookForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.booksService.createBook(this.toBookRequest()).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: book => { this.books.update(books => [...books, book].sort((a, b) => a.name.localeCompare(b.name))); this.bookForm.reset(); this.errorMessage.set(''); },
      error: () => this.errorMessage.set('Book could not be added. Check the details and try again.')
    });
  }

  editBook(book: Book): void {
    this.editingBook.set(book);
    const relativePath = book.filePath.startsWith(`${this.libraryRoot}\\`)
      ? book.filePath.slice(this.libraryRoot.length + 1)
      : book.filePath;
    const pathParts = relativePath.split(/[\\/]/);
    pathParts.pop();
    const subfolder = pathParts.join('\\');
    const extension = /\.([^.]+)$/.exec(book.filePath)?.[1]?.toUpperCase() ?? book.fileType;
    const supportedType = ['PDF', 'EPUB', 'MOBI'].includes(extension) ? extension : 'PDF';
    this.bookForm.setValue({ name: book.name, subfolder, fileType: supportedType });
    this.errorMessage.set('');
  }

  cancelEdit(): void { this.editingBook.set(null); this.bookForm.reset(); this.errorMessage.set(''); }

  saveBook(): void {
    if (this.bookForm.invalid || this.saving()) { this.bookForm.markAllAsTouched(); return; }
    const book = this.editingBook();
    if (!book) { this.addBook(); return; }
    this.saving.set(true);
    this.booksService.updateBook(book.id, this.toBookRequest()).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updatedBook => {
        this.books.update(books => books.map(item => item.id === updatedBook.id ? updatedBook : item).sort((a, b) => a.name.localeCompare(b.name)));
        this.cancelEdit();
      },
      error: () => this.errorMessage.set('Book could not be updated. Check the details and try again.')
    });
  }

  private toBookRequest(): { name: string; fileType: string; filePath: string } {
    const value = this.bookForm.getRawValue();
    const segments = [this.libraryRoot, value.subfolder.trim(), `${value.name.trim()}.${value.fileType.toLowerCase()}`]
      .filter(segment => segment.length > 0);
    return { name: value.name.trim(), fileType: value.fileType, filePath: segments.join('\\') };
  }

  private openBookError(error: unknown, book: Book): string {
    const fallback = `Could not open ${book.name}. Check that the file exists and Windows has an app assigned to .${book.fileType.toLowerCase()} files.`;
    if (!(error instanceof HttpErrorResponse)) return fallback;

    const problem = error.error as { detail?: unknown; title?: unknown } | null;
    if (problem && typeof problem.detail === 'string') return problem.detail;
    if (problem && typeof problem.title === 'string') return problem.title;
    return fallback;
  }

  private importError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const problem = error.error as { detail?: unknown; title?: unknown } | null;
      if (problem && typeof problem.detail === 'string') return problem.detail;
      if (problem && typeof problem.title === 'string') return problem.title;
    }
    return 'Could not import books. Check that the folder exists and is accessible.';
  }
}
