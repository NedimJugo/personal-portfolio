export interface EmailTemplateUpdateRequest {
  name?: string;
  subject?: string;
  htmlContent?: string;
  textContent?: string;
  isActive?: boolean;
}