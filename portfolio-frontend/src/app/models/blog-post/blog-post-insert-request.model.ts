import { BlogPostStatus } from "../enums/blog-post-status.enum";

export interface BlogPostInsertRequest {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  featuredImage?: string;
  status: BlogPostStatus;
  tags: string;        // JSON string, e.g., "[]"
  categories: string;  // JSON string, e.g., "[]"
  createdById: number;
  projectId?: string;  // Guid as string
}