import { BlogPostStatus } from "../enums/blog-post-status.enum";

export interface BlogPostResponse {
  id: string;          // Guid as string
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  featuredImage?: string;
  status: BlogPostStatus;
  tags: string;
  categories: string;
  readingTime: number;
  viewCount: number;
  likeCount: number;
  createdById: number;
  createdAt: string;     // ISO date string
  updatedAt: string;     // ISO date string
  publishedAt?: string;  // nullable ISO date string
}
