import { ProjectImageInsertRequest } from "../project-image/project-image-insert-request.model";

export interface ProjectInsertRequest {
  title: string;
  slug: string;
  shortDescription: string;
  fullDescription: string;
  projectType: string;            // default: "web"
  status: string;                 // default: "completed"
  isFeatured: boolean;
  featuredMediaId?: string;       // Guid as string
  repoUrl?: string;
  liveUrl?: string;
  startDate?: string;             // DateOnly as ISO string
  endDate?: string;               // DateOnly as ISO string
  isPublished: boolean;
  displayOrder: number;
  createdById: number;
  updatedById: number;

  // Relations
  images: ProjectImageInsertRequest[];
  tagIds: string[];               // Guid[] as string[]
  techIds: string[];              // Guid[] as string[]
}
