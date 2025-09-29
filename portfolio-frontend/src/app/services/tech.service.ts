import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { TechResponse } from '../models/tech/tech-response.model';
import { TechSearchObject } from '../models/tech/tech-search.model';
import { TechInsertRequest } from '../models/tech/tech-insert-request.model';
import { TechUpdateRequest } from '../models/tech/tech-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class TechService extends BaseCrudService<
  TechResponse,
  TechSearchObject,
  TechInsertRequest,
  TechUpdateRequest,
  string
> {
  
  protected endpoint = 'techs';

  constructor(http: HttpClient) {
    super(http);
  }
}