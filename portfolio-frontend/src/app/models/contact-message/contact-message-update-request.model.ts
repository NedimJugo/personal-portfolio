export interface ContactMessageUpdateRequest {
  subject?: string;
  message?: string;
  phone?: string;
  company?: string;
  projectType?: string;
  budgetRange?: string;
  source?: string;
  status?: string;
  priority?: string;
  handledById?: number;
}