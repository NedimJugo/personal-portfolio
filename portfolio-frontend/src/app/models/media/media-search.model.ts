import { BaseSearchObject } from "../base/base-search-object.model";

export interface MediaSearchObject extends BaseSearchObject {
  fileName?: string;
  fileType?: string;
  storageProvider?: string;
  uploadedById?: number;
}
