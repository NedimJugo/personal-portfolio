import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { SkillResponse } from '../models/skill/skill-response.model';
import { SkillSearchObject } from '../models/skill/skill-search.model';
import { SkillInsertRequest } from '../models/skill/skill-insert-request.model';
import { SkillUpdateRequest } from '../models/skill/skill-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class SkillService extends BaseCrudService<
  SkillResponse,
  SkillSearchObject,
  SkillInsertRequest,
  SkillUpdateRequest,
  string
> {
  
  protected endpoint = 'skills';

  constructor(http: HttpClient) {
    super(http);
  }
}