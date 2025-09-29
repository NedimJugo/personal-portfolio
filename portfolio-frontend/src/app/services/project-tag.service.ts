import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ProjectTagResponse } from '../models/project-tag/project-tag-response.model';
import { ProjectTagSearchObject } from '../models/project-tag/project-tag-search.model';
import { ProjectTagInsertRequest } from '../models/project-tag/project-tag-insert-request.model';
import { ProjectTagUpdateRequest } from '../models/project-tag/project-tag-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectTagService extends BaseCrudService<
  ProjectTagResponse,
  ProjectTagSearchObject,
  ProjectTagInsertRequest,
  ProjectTagUpdateRequest,
  string
> {
  
  protected endpoint = 'projecttags';

  constructor(http: HttpClient) {
    super(http);
  }
}