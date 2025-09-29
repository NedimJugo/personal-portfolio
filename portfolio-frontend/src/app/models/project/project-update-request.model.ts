import { ProjectImageUpdateRequest } from "../project-image/project-image-update-request.model";

export interface ProjectUpdateRequest {
  title?: string;
  slug?: string;
  shortDescription?: string;
  fullDescription?: string;
  projectType?: string;
  status?: string;
  isFeatured?: boolean;
  featuredMediaId?: string;
  repoUrl?: string;
  liveUrl?: string;
  startDate?: string;
  endDate?: string;
  isPublished?: boolean;
  displayOrder?: number;
  updatedById: number;

  // Relations
  images?: ProjectImageUpdateRequest[];
  tagIds?: string[];
  techIds?: string[];
}
