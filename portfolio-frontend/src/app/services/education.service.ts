import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { EducationResponse } from '../models/education/education-response.model';
import { EducationSearchObject } from '../models/education/education-search-object.model';
import { EducationInsertRequest } from '../models/education/education-insert-request.model';
import { EducationUpdateRequest } from '../models/education/education-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class EducationService extends BaseCrudService<
  EducationResponse,
  EducationSearchObject,
  EducationInsertRequest,
  EducationUpdateRequest,
  string
> {
  
  protected endpoint = 'educations';

  constructor(http: HttpClient) {
    super(http);
  }
}