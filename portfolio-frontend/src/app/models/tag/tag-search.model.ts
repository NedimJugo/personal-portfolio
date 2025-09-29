import { BaseSearchObject } from "../base/base-search-object.model";

export interface TagSearchObject extends BaseSearchObject {
  name?: string;
  slug?: string;
}