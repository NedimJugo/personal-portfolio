import { BaseSearchObject } from "../base/base-search-object.model";

export interface TechSearchObject extends BaseSearchObject {
  name?: string;
  slug?: string;
  category?: string;
}