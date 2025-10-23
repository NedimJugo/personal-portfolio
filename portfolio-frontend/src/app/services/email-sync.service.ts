import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailSyncService {
  private readonly apiUrl = `${environment.apiUrl}/emailsync`;

  constructor(private http: HttpClient) {}

  syncEmails(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/sync`, {});
  }

  importEmail(messageId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/import/${messageId}`, {});
  }
}