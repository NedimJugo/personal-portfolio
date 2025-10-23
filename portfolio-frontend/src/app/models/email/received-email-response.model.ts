export interface ReceivedEmailResponse {
  messageId: string;
  from: string;
  fromName: string;
  subject: string;
  body: string;
  replyTo?: string;
  receivedDate: string;   // DateTimeOffset from backend → string in JSON
  isReply: boolean;
  inReplyTo?: string;
}
