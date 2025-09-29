export interface TestimonialUpdateRequest {
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
}