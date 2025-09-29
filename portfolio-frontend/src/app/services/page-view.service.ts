import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { PageViewResponse } from '../models/page-view/page-view-response.model';
import { PageViewSearchObject } from '../models/page-view/page-view-search.model';
import { PageViewInsertRequest } from '../models/page-view/page-view-insert-request.model';
import { PageViewUpdateRequest } from '../models/page-view/page-view-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class PageViewService extends BaseCrudService<
  PageViewResponse,
  PageViewSearchObject,
  PageViewInsertRequest,
  PageViewUpdateRequest,
  string
> {
  
  protected endpoint = 'pageviews';

  constructor(http: HttpClient) {
    super(http);
  }
}