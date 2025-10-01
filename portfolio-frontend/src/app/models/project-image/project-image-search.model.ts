  import { BaseSearchObject } from "../base/base-search-object.model";

  export interface ProjectImageSearchObject extends BaseSearchObject {
    projectId?: string;   // Guid as string
    mediaId?: string;     // Guid as string
    isHero?: boolean;
  }
