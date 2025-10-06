import { CertificateResponse } from "./certificate-response.model";

export interface CertificateWithLogos extends CertificateResponse {
  logoUrl?: string;
  certificateUrl?: string;
}