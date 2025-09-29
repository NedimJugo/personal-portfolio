import { EmploymentType } from "../enums/employment-type.enum";

export interface ExperienceUpdateRequest {
  companyName?: string;
  companyLogo?: string;
  position?: string;
  location?: string;
  employmentType?: EmploymentType;
  startDate?: string;      // nullable DateOnly
  endDate?: string;        // nullable DateOnly
  isCurrent?: boolean;
  description?: string;
  achievements?: string;
  technologies?: string;
  displayOrder?: number;
}