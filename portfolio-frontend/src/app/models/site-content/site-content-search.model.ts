import { BaseSearchObject } from "../base/base-search-object.model";

export interface SiteContentSearchObject extends BaseSearchObject {
  section?: string;
  contentType?: 'text' | 'html' | 'json';
  isPublished?: boolean;
}