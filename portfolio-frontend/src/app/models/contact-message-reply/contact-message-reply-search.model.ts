import { BaseSearchObject } from "../base/base-search-object.model";

export interface ContactMessageReplySearchObject extends BaseSearchObject {
  contactMessageId?: string;       // Guid as string
  repliedById?: number;
  deliveryStatus?: string;
  isInternal?: boolean;
  repliedFrom?: string;            // ISO date string
  repliedTo?: string;              // ISO date string
}