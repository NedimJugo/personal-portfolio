export interface ContactMessageReplyResponse {
  id: string;                      // Guid as string
  contactMessageId: string;        // Guid as string
  replyMessage: string;
  replyToEmail?: string;
  subject?: string;
  repliedById: number;
  repliedAt: string;               // ISO date string
  deliveryStatus: string;          // 'sent' | 'failed' | 'pending'
  deliveredAt?: string;            // nullable ISO date string
  errorMessage?: string;
  isInternal: boolean;
  createdAt: string;               // ISO date string
  updatedAt: string;               // ISO date string
}