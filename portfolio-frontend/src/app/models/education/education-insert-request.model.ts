export interface EducationInsertRequest {
  institutionName: string;
  degree: string;
  fieldOfStudy?: string;
  location?: string;
  startDate: string;           // ISO date string
  endDate?: string;            // nullable ISO date string
  isCurrent: boolean;
  grade?: string;
  description?: string;
  educationType: string;
  displayOrder: number;
  logoMediaId?: string;        // Guid as string
}