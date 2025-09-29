import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ApplicationUserResponse } from '../models/application-user/application-user-response.model';
import { ApplicationUserSearchObject } from '../models/application-user/application-user-search.model';
import { ApplicationUserInsertRequest } from '../models/application-user/application-user-insert-request.model';
import { ApplicationUserUpdateRequest } from '../models/application-user/application-user-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ApplicationUserService extends BaseCrudService<
  ApplicationUserResponse,
  ApplicationUserSearchObject,
  ApplicationUserInsertRequest,
  ApplicationUserUpdateRequest,
  number
> {

  protected endpoint = 'applicationusers';

  constructor(http: HttpClient) {
    super(http);
  }
}