import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { SocialLinkResponse } from '../models/social-link/social-link-response.model';
import { SocialLinkSearchObject } from '../models/social-link/social-link-search.model';
import { SocialLinkInsertRequest } from '../models/social-link/social-link-insert-request.model';
import { SocialLinkUpdateRequest } from '../models/social-link/social-link-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class SocialLinkService extends BaseCrudService<
  SocialLinkResponse,
  SocialLinkSearchObject,
  SocialLinkInsertRequest,
  SocialLinkUpdateRequest,
  string
> {
  
  protected endpoint = 'sociallinks';

  constructor(http: HttpClient) {
    super(http);
  }
}