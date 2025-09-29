import { BaseSearchObject } from "../base/base-search-object.model";

export interface ProjectTagSearchObject extends BaseSearchObject {
  projectId?: string;
  tagId?: string;
}