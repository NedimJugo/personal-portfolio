export interface EmailTemplateInsertRequest {
  name: string;
  subject: string;
  htmlContent: string;
  textContent: string;
  isActive: boolean;
}