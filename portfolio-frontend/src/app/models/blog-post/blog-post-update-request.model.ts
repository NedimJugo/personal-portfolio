import { BlogPostStatus } from "../enums/blog-post-status.enum";

export interface BlogPostUpdateRequest {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  featuredImage?: string;
  status: BlogPostStatus;
  tags: string;
  categories: string;
  projectId?: string;
}