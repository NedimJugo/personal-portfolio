export interface ContactMessageResponse {
  id: string;
  name: string;
  email: string;
  subject: string;
  message: string;
  phone?: string;
  company?: string;
  projectType?: string;
  budgetRange?: string;
  ipAddress?: string;
  userAgent?: string;
  source?: string;
  status: string;
  priority: string;
  handledById?: number;
  createdAt: Date;
  updatedAt: Date;
}