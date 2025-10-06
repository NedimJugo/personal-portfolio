export interface EducationResponse {
  id: string;                  // Guid as string
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
  createdAt: string;           // ISO date string
  updatedAt: string;           // ISO date string
}