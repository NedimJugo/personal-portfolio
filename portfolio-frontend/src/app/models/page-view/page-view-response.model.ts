export interface PageViewResponse {
  id: string;           // Guid as string
  path: string;
  referrer?: string;
  userAgent?: string;
  ipAddress?: string;
  country?: string;
  city?: string;
  viewedAt: string;     // ISO date string
  projectId?: string;
  blogPostId?: string;
}
