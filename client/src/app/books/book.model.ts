export interface Book {
  id: number;
  name: string;
  fileType: string;
  filePath: string;
}

export interface BookRequest {
  name: string;
  fileType: string;
  filePath: string;
}

export interface BookImportResponse {
  importedCount: number;
  skippedCount: number;
}

export type CreateBookRequest = BookRequest;
export type UpdateBookRequest = BookRequest;
