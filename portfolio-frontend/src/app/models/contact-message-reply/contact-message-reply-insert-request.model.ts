export interface ContactMessageReplyInsertRequest {
  contactMessageId: string;
  replyMessage: string;
  replyToEmail?: string;
  subject?: string;
  isInternal: boolean;
}