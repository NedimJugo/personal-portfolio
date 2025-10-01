import { ProjectResponse } from "../project/project-response.model";

export interface ProjectWithImage extends ProjectResponse {
  heroImageUrl?: string;
  heroImageAlt?: string;
}