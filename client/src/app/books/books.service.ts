import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Book, BookImportResponse, BookRequest } from './book.model';

@Injectable({ providedIn: 'root' })
export class BooksService {
  private readonly http = inject(HttpClient);
  private readonly booksUrl = '/api/books';

  getBooks(): Observable<Book[]> { return this.http.get<Book[]>(this.booksUrl); }
  createBook(request: BookRequest): Observable<Book> { return this.http.post<Book>(this.booksUrl, request); }
  importBooks(folderPath: string): Observable<BookImportResponse> {
    return this.http.post<BookImportResponse>(`${this.booksUrl}/import`, { folderPath });
  }
  bookFileUrl(id: number, name: string, fileType: string): string {
    const fileName = `${name}.${fileType.toLowerCase()}`;
    return `${this.booksUrl}/${id}/file/${encodeURIComponent(fileName)}`;
  }
  openBook(id: number): Observable<void> { return this.http.post<void>(`${this.booksUrl}/${id}/open`, {}); }
  updateBook(id: number, request: BookRequest): Observable<Book> {
    return this.http.put<Book>(`${this.booksUrl}/${id}`, request);
  }
}
