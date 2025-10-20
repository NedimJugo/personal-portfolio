  import { BaseSearchObject } from "../base/base-search-object.model";

  export interface SubscriberSearchObject extends BaseSearchObject {
    email?: string;
    isActive?: boolean;
    source?: string;
  }