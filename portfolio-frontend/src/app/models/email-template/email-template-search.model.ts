import { BaseSearchObject } from "../base/base-search-object.model";

export interface EmailTemplateSearchObject extends BaseSearchObject {
  name?: string;
  isActive?: boolean;
}