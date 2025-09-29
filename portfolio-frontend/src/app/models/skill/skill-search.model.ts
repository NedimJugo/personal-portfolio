import { BaseSearchObject } from "../base/base-search-object.model";

export interface SkillSearchObject extends BaseSearchObject {
  name?: string;
  category?: string;
  isFeatured?: boolean;
  minProficiency?: number;
  maxProficiency?: number;
}