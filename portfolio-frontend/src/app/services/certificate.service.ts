import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { CertificateResponse } from '../models/certificate/certificate-response.model';
import { CertificateSearchObject } from '../models/certificate/certificate-search-object.model';
import { CertificateInsertRequest } from '../models/certificate/certificate-insert-request.model';
import { CertificateUpdateRequest } from '../models/certificate/certificate-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class CertificateService extends BaseCrudService<
  CertificateResponse,
  CertificateSearchObject,
  CertificateInsertRequest,
  CertificateUpdateRequest,
  string
> {
  
  protected endpoint = 'certificates';

  constructor(http: HttpClient) {
    super(http);
  }
}