export interface CertificateUpdateRequest {
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
}