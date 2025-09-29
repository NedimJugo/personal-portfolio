export interface EmailTemplateResponse {
  id: string;           // Guid as string
  name: string;
  subject: string;
  htmlContent: string;
  textContent: string;
  isActive: boolean;
  createdAt: string;    // ISO date string
  updatedAt: string;    // ISO date string
}