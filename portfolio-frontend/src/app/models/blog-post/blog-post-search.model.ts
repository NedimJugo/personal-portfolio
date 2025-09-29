import { BaseSearchObject } from "../base/base-search-object.model";
import { BlogPostStatus } from "../enums/blog-post-status.enum";

export interface BlogPostSearchObject extends BaseSearchObject {
  title?: string;
  status?: BlogPostStatus;
  createdById?: number;
  projectId?: string;
}