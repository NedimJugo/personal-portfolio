import { MediaResponse } from "../media/media-response.model";

export interface TechResponse {
  id: string;
  name: string;
  slug: string;
  category: string;
  iconMediaId?: string;
  
  // Navigation
  iconMedia?: MediaResponse;
}