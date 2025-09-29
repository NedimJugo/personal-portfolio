export interface SubscriberInsertRequest {
  email: string;
  name?: string;
  isActive: boolean;
  source?: string;
}