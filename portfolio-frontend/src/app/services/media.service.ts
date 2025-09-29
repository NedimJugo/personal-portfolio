import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { MediaResponse } from '../models/media/media-response.model';
import { MediaSearchObject } from '../models/media/media-search.model';
import { MediaInsertRequest } from '../models/media/media-insert-request.model';
import { MediaUpdateRequest } from '../models/media/media-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class MediaService extends BaseCrudService<
  MediaResponse,
  MediaSearchObject,
  MediaInsertRequest,
  MediaUpdateRequest,
  string
> {
  
  protected endpoint = 'media';

  constructor(http: HttpClient) {
    super(http);
  }
}