import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { SubscriberResponse } from '../models/subscriber/subscriber-response.model';
import { SubscriberSearchObject } from '../models/subscriber/subscriber-search.model';
import { SubscriberInsertRequest } from '../models/subscriber/subscriber-insert-request.model';
import { SubscriberUpdateRequest } from '../models/subscriber/subscriber-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class SubscriberService extends BaseCrudService<
  SubscriberResponse,
  SubscriberSearchObject,
  SubscriberInsertRequest,
  SubscriberUpdateRequest,
  string
> {
  
  protected endpoint = 'subscribers';

  constructor(http: HttpClient) {
    super(http);
  }
}