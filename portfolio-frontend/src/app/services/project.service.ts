  import { Injectable } from '@angular/core';
  import { HttpClient } from '@angular/common/http';
  import { BaseCrudService } from './base/base-crud.service';
  import { ProjectResponse } from '../models/project/project-response.model';
  import { ProjectSearchObject } from '../models/project/project-search.model';
  import { ProjectInsertRequest } from '../models/project/project-insert-request.model';
  import { ProjectUpdateRequest } from '../models/project/project-update-request.model';

  @Injectable({
    providedIn: 'root'
  })
  export class ProjectService extends BaseCrudService<
    ProjectResponse,
    ProjectSearchObject,
    ProjectInsertRequest,
    ProjectUpdateRequest,
    string
  > {
    
    protected endpoint = 'projects';

    constructor(http: HttpClient) {
      super(http);
    }
  }