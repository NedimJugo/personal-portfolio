import { BaseSearchObject } from "../base/base-search-object.model";

export interface EducationSearchObject extends BaseSearchObject {
  institutionName?: string;
  degree?: string;
  educationType?: string;
  isCurrent?: boolean;
}