import { BaseSearchObject } from "../base/base-search-object.model";

export interface ProjectTechSearchObject extends BaseSearchObject {
  projectId?: string;
  techId?: string;
}