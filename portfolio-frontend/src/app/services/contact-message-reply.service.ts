import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseCrudService } from './base/base-crud.service';
import { ContactMessageReplyResponse } from '../models/contact-message-reply/contact-message-reply-response.model';
import { ContactMessageReplyInsertRequest } from '../models/contact-message-reply/contact-message-reply-insert-request.model';
import { ContactMessageReplyUpdateRequest } from '../models/contact-message-reply/contact-message-reply-update-request.model';
import { ContactMessageReplySearchObject } from '../models/contact-message-reply/contact-message-reply-search.model';

@Injectable({
  providedIn: 'root'
})
export class ContactMessageReplyService extends BaseCrudService<
  ContactMessageReplyResponse,
  ContactMessageReplySearchObject,
  ContactMessageReplyInsertRequest,
  ContactMessageReplyUpdateRequest,
  string
> {
  
  protected endpoint = 'ContactMessageReplies';

  constructor(http: HttpClient) {
    super(http);
  }

}