import { BaseSearchObject } from "../base/base-search-object.model";

export interface ProjectSearchObject extends BaseSearchObject {
  title?: string;
  status?: string;
  projectType?: string;
  isFeatured?: boolean;
  isPublished?: boolean;
}