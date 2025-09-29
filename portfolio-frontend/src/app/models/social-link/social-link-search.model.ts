import { BaseSearchObject } from "../base/base-search-object.model";

export interface SocialLinkSearchObject extends BaseSearchObject {
  platform?: string;
  isVisible?: boolean;
}