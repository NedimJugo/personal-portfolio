import { BaseSearchObject } from "../base/base-search-object.model";

export interface ApplicationUserSearchObject extends BaseSearchObject {
  userName?: string;
  email?: string;
  isActive?: boolean;
}