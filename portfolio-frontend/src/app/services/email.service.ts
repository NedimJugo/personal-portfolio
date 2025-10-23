import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ContactMessageReplyResponse } from '../models/contact-message-reply/contact-message-reply-response.model';
import { ReceivedEmailResponse } from '../models/email/received-email-response.model';
import { SendEmailRequest } from '../models/email/send-email-request.model';


@Injectable({
  providedIn: 'root'
})
export class EmailService {
  private baseUrl = `${environment.apiUrl}/email`;

  constructor(private http: HttpClient) {}


  fetchNewEmails(): Observable<ReceivedEmailResponse[]> {
    return this.http.get<ReceivedEmailResponse[]>(`${this.baseUrl}/fetch`);
  }

  markEmailAsRead(messageId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/mark-read/${messageId}`, {});
  }

  sendEmail(request: SendEmailRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/send`, request);
  }

  sendReply(reply: ContactMessageReplyResponse): Observable<any> {
    return this.http.post(`${this.baseUrl}/reply`, reply);
  }
}
