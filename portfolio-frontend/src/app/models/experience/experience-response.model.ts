import { EmploymentType } from "../enums/employment-type.enum";

export interface ExperienceResponse {
  id: string;               // Guid as string
  companyName: string;
  companyLogo?: string;
  position: string;
  location: string;
  employmentType: EmploymentType;
  startDate: string;        // DateOnly as string
  endDate?: string;         // nullable
  isCurrent: boolean;
  description: string;
  achievements: string;
  technologies: string;
  displayOrder: number;
  createdAt: string;        // ISO date string
  updatedAt: string;        // ISO date string
}