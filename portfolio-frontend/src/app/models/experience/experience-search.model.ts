import { BaseSearchObject } from "../base/base-search-object.model";
import { EmploymentType } from "../enums/employment-type.enum";

export interface ExperienceSearchObject extends BaseSearchObject {
  companyName?: string;
  position?: string;
  employmentType?: EmploymentType;
  isCurrent?: boolean;
}
