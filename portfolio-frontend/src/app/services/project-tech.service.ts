import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ProjectTechResponse } from '../models/project-tech/project-tech-response.model';
import { ProjectTechSearchObject } from '../models/project-tech/project-tech-search.model';
import { ProjectTechInsertRequest } from '../models/project-tech/project-tech-insert-request.model';
import { ProjectTechUpdateRequest } from '../models/project-tech/project-tech-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectTechService extends BaseCrudService<
  ProjectTechResponse,
  ProjectTechSearchObject,
  ProjectTechInsertRequest,
  ProjectTechUpdateRequest,
  string
> {
  
  protected endpoint = 'projecttechs';

  constructor(http: HttpClient) {
    super(http);
  }
}