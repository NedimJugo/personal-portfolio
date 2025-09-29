import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { SiteContentResponse } from '../models/site-content/site-content-response.model';
import { SiteContentSearchObject } from '../models/site-content/site-content-search.model';
import { SiteContentInsertRequest } from '../models/site-content/site-content-insert-request.model';
import { SiteContentUpdateRequest } from '../models/site-content/site-content-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class SiteContentService extends BaseCrudService<
  SiteContentResponse,
  SiteContentSearchObject,
  SiteContentInsertRequest,
  SiteContentUpdateRequest,
  string
> {
  
  protected endpoint = 'sitecontent';

  constructor(http: HttpClient) {
    super(http);
  }
}