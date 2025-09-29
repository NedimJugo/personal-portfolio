export interface SubscriberResponse {
  id: string;
  email: string;
  name?: string;
  isActive: boolean;
  source?: string;
  subscribedAt: Date;
  unsubscribedAt?: Date;
}