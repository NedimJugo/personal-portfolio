export interface CertificateResponse {
  id: string;                  // Guid as string
  name: string;
  issuingOrganization: string;
  issueDate: string;           // ISO date string
  expirationDate?: string;     // nullable ISO date string
  credentialId?: string;
  credentialUrl?: string;
  description?: string;
  skills?: string;
  certificateType: string;
  isActive: boolean;
  isPublished: boolean;
  displayOrder: number;
  logoMediaId?: string;        // Guid as string
  certificateMediaId?: string; // Guid as string
  createdAt: string;           // ISO date string
  updatedAt: string;           // ISO date string
}