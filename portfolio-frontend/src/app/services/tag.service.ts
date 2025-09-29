import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { TagResponse } from '../models/tag/tag-response.model';
import { TagSearchObject } from '../models/tag/tag-search.model';
import { TagInsertRequest } from '../models/tag/tag-insert-request.model';
import { TagUpdateRequest } from '../models/tag/tag-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class TagService extends BaseCrudService<
  TagResponse,
  TagSearchObject,
  TagInsertRequest,
  TagUpdateRequest,
  string
> {
  
  protected endpoint = 'tags';

  constructor(http: HttpClient) {
    super(http);
  }
}