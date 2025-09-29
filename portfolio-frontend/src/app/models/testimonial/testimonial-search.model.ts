import { BaseSearchObject } from "../base/base-search-object.model";

export interface TestimonialSearchObject extends BaseSearchObject {
  clientName?: string;
  isApproved?: boolean;
  isFeatured?: boolean;
  projectId?: string;
}