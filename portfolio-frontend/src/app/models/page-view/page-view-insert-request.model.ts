export interface PageViewInsertRequest {
  path: string;
  referrer?: string;
  userAgent?: string;
  ipAddress?: string;
  country?: string;
  city?: string;
  visitorKey?: string;  // Add this
  projectId?: string;   // Guid as string
  blogPostId?: string;  // Guid as string
}
