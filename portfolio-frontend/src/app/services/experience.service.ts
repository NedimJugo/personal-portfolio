import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { ExperienceResponse } from '../models/experience/experience-response.model';
import { ExperienceSearchObject } from '../models/experience/experience-search.model';
import { ExperienceInsertRequest } from '../models/experience/experience-insert-request.model';
import { ExperienceUpdateRequest } from '../models/experience/experience-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class ExperienceService extends BaseCrudService<
  ExperienceResponse,
  ExperienceSearchObject,
  ExperienceInsertRequest,
  ExperienceUpdateRequest,
  string
> {
  
  protected endpoint = 'experiences';

  constructor(http: HttpClient) {
    super(http);
  }
}