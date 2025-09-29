import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ProjectImageResponse } from '../models/project-image/project-image-response.model';
import { ProjectImageSearchObject } from '../models/project-image/project-image-search.model';
import { ProjectImageInsertRequest } from '../models/project-image/project-image-insert-request.model';
import { ProjectImageUpdateRequest } from '../models/project-image/project-image-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectImageService extends BaseCrudService<
  ProjectImageResponse,
  ProjectImageSearchObject,
  ProjectImageInsertRequest,
  ProjectImageUpdateRequest,
  string
> {
  
  protected endpoint = 'projectimages';

  constructor(http: HttpClient) {
    super(http);
  }
}