import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpParams, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BaseCrudService } from './base/base-crud.service';
import { MediaResponse } from '../models/media/media-response.model';
import { MediaSearchObject } from '../models/media/media-search.model';
import { MediaInsertRequest } from '../models/media/media-insert-request.model';
import { MediaUpdateRequest } from '../models/media/media-update-request.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MediaService extends BaseCrudService<
  MediaResponse,
  MediaSearchObject,
  MediaInsertRequest,
  MediaUpdateRequest,
  string
> {
  protected endpoint = 'media';
  private apiUrl = `${environment.apiUrl}/${this.endpoint}`;

  constructor(http: HttpClient) {
    super(http);
  }

  uploadFile(
    file: File,
    altText?: string,
    caption?: string,
    folder?: string
  ): Observable<MediaResponse> {
    const formData = new FormData();
    formData.append('file', file);
    if (altText) formData.append('altText', altText);
    if (caption) formData.append('caption', caption);
    if (folder) formData.append('folder', folder);

    return this.http.post<MediaResponse>(`${this.apiUrl}/upload`, formData)
      .pipe(catchError(this.handleError));
  }

  uploadMultipleFiles(
    files: File[],
    folder?: string
  ): Observable<MediaResponse[]> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    if (folder) formData.append('folder', folder);

    return this.http.post<MediaResponse[]>(`${this.apiUrl}/upload/bulk`, formData)
      .pipe(catchError(this.handleError));
  }

  downloadFile(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, {
      responseType: 'blob'
    }).pipe(catchError(this.handleError));
  }

  downloadFileByName(fileName: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${fileName}`, {
      responseType: 'blob'
    }).pipe(catchError(this.handleError));
  }

  generateSasUrl(id: string, expiryHours: number = 1): Observable<{
    mediaId: string;
    fileName: string;
    originalFileName: string;
    sasUrl: string;
    expiresAt: string;
  }> {
    const params = new HttpParams().set('expiryHours', expiryHours.toString());
    return this.http.get<{
      mediaId: string;
      fileName: string;
      originalFileName: string;
      sasUrl: string;
      expiresAt: string;
    }>(`${this.apiUrl}/${id}/sas-url`, { params })
      .pipe(catchError(this.handleError));
  }

  listFiles(folder?: string): Observable<string[]> {
    const url = folder ? `${this.apiUrl}/list/${folder}` : `${this.apiUrl}/list`;
    return this.http.get<string[]>(url)
      .pipe(catchError(this.handleError));
  }

  uploadFileWithProgress(
    file: File,
    folder?: string
  ): Observable<HttpEvent<MediaResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    if (folder) formData.append('folder', folder);

    const req = new HttpRequest('POST', `${this.apiUrl}/upload`, formData, {
      reportProgress: true,
    });

    return this.http.request<MediaResponse>(req)
      .pipe(catchError(this.handleError));
  }
}
