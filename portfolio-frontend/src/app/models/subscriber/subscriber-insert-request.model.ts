export interface SubscriberInsertRequest {
  email: string;
  name?: string;
  isActive: boolean;
  source?: string;
  /** Honeypot anti-spam field - must stay empty, hidden from real users via CSS. */
  website?: string;
}