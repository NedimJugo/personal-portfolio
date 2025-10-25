import { ProjectImageResponse } from "../project-image/project-image-response.model";

export interface ProjectResponse {
  id: string;
  title: string;
  slug: string;
  shortDescription: string;
  fullDescription: string;
  projectType: string;
  status: string;
  isFeatured: boolean;
  featuredMediaId?: string;
  repoUrl?: string;
  liveUrl?: string;
  startDate?: string;
  endDate?: string;
  isPublished: boolean;
  publishedAt?: string;          // ISO date string
  displayOrder: number;
  viewCount: number;
  createdById: number;
  updatedById: number;
  createdAt: string;             // ISO date string
  updatedAt: string;             // ISO date string
  
  // Relations
  images: ProjectImageResponse[];
  tagIds: string[];
  techIds: string[];
}
