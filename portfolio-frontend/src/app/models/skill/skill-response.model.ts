import { MediaResponse } from "../media/media-response.model";

export interface SkillResponse {
  id: string;
  name: string;
  category: string;
  proficiencyLevel: number;
  yearsExperience: number;
  isFeatured: boolean;
  iconMediaId?: string;
  color?: string;
  displayOrder: number;
  createdAt: Date;
  updatedAt: Date;
  
  // Navigation
  iconMedia?: MediaResponse;
}