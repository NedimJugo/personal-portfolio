import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { SettingsResponse } from '../models/setting/setting-response.model';
import { SettingsSearchObject } from '../models/setting/setting-search.model';
import { SettingsInsertRequest } from '../models/setting/setting-insert-request.model';
import { SettingsUpdateRequest } from '../models/setting/setting-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class SettingsService extends BaseCrudService<
  SettingsResponse,
  SettingsSearchObject,
  SettingsInsertRequest,
  SettingsUpdateRequest,
  string
> {
  
  protected endpoint = 'settings';

  constructor(http: HttpClient) {
    super(http);
  }
}