export interface ProjectImageUpdateRequest {
  projectId: string;  // Guid as string
  mediaId: string;    // Guid as string
  caption?: string;
  order?: number;
  isHero?: boolean;
}
