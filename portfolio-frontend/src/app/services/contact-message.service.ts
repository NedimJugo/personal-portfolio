import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ContactMessageResponse } from '../models/contact-message/contact-message-response.model';
import { ContactMessageSearchObject } from '../models/contact-message/contact-message-search.model';
import { ContactMessageInsertRequest } from '../models/contact-message/contact-message-insert-request.model';
import { ContactMessageUpdateRequest } from '../models/contact-message/contact-message-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ContactMessageService extends BaseCrudService<
  ContactMessageResponse,
  ContactMessageSearchObject,
  ContactMessageInsertRequest,
  ContactMessageUpdateRequest,
  string
> {
  protected endpoint = 'contactmessages';

  constructor(http: HttpClient) {
    super(http);
  }
}