import { BaseSearchObject } from "../base/base-search-object.model";

export interface ContactMessageSearchObject extends BaseSearchObject {
  name?: string;
  email?: string;
  subject?: string;
  status?: string;
  priority?: string;
  handledById?: number;
}