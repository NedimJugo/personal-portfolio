export interface ContactMessageReplyUpdateRequest {
  replyMessage: string;
  replyToEmail?: string;
  subject?: string;
  deliveryStatus: string;          // 'sent' | 'failed' | 'pending'
  deliveredAt?: string;            // nullable ISO date string
  errorMessage?: string;
  isInternal: boolean;
}