import { BaseSearchObject } from "../base/base-search-object.model";

export interface CertificateSearchObject extends BaseSearchObject {
  name?: string;
  issuingOrganization?: string;
  certificateType?: string;
  isActive?: boolean;
  isPublished?: boolean;
}