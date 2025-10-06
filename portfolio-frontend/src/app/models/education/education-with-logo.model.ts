import { EducationResponse } from "./education-response.model";

export interface EducationWithLogo extends EducationResponse {
  logoUrl?: string;
}