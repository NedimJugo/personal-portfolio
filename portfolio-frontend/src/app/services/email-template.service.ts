import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { EmailTemplateResponse } from '../models/email-template/email-template-response.model';
import { EmailTemplateSearchObject } from '../models/email-template/email-template-search.model';
import { EmailTemplateInsertRequest } from '../models/email-template/email-template-insert-request.model';
import { EmailTemplateUpdateRequest } from '../models/email-template/email-template-update-request.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EmailTemplateService extends BaseCrudService<
  EmailTemplateResponse,
  EmailTemplateSearchObject,
  EmailTemplateInsertRequest,
  EmailTemplateUpdateRequest,
  string
> {
  
  protected endpoint = 'emailtemplates';

  constructor(http: HttpClient) {
    super(http);
  }

  sendToSubscribers(templateId: string, emails: string[]): Observable<any> {
  return this.http.post(`${this.baseUrl}/${this.endpoint}/${templateId}/send`, { emails });
}
}