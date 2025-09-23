import { Tag } from "./tag";
import { Tech } from "./tech";

export interface Project {
  id: number;
  title: string;
  slug: string;
  shortDescription: string;
  content?: string;
  featuredMediaUrl?: string;
  repoUrl?: string;
  liveUrl?: string;
  isPublished: boolean;
  publishedAt?: Date;
  views: number;
  techs: Tech[];
  tags: Tag[];
  createdAt: Date;
  updatedAt: Date;
}