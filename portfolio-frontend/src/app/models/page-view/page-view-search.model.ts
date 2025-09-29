import { BaseSearchObject } from "../base/base-search-object.model";

export interface PageViewSearchObject extends BaseSearchObject {
  path?: string;
  ipAddress?: string;
  country?: string;
  projectId?: string;
  blogPostId?: string;
}
