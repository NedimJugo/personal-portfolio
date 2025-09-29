export interface SubscriberUpdateRequest {
  email: string;
  name?: string;
  isActive: boolean;
  source?: string;
  unsubscribedAt?: Date;
}