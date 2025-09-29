import { EmploymentType } from "../enums/employment-type.enum";

export interface ExperienceInsertRequest {
  companyName: string;
  companyLogo?: string;
  position: string;
  location: string;
  employmentType: EmploymentType;
  startDate: string;      // DateOnly as ISO date string, e.g., "2025-01-01"
  endDate?: string;       // nullable DateOnly
  isCurrent: boolean;
  description: string;
  achievements: string;   // JSON string, e.g., "[]"
  technologies: string;   // JSON string, e.g., "[]"
  displayOrder: number;
}
