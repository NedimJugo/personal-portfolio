import { ProjectResponse } from "../project/project-response.model";

export interface TestimonialResponse {
  id: string;
  clientName: string;
  clientTitle: string;
  clientCompany: string;
  clientAvatar?: string;
  content: string;
  rating: number;
  isApproved: boolean;
  isFeatured: boolean;
  displayOrder: number;
  projectId?: string;
  createdAt: Date;
  updatedAt: Date;
  
  // Soft delete metadata
  isDeleted: boolean;
  deletedAt?: Date;
  deletedById?: number;
  
  // Navigation
  project?: ProjectResponse;
}