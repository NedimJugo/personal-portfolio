export interface ApplicationUserInsertRequest {
  userName: string;
  email: string;
  fullName: string;
  isActive: boolean;
  password: string; // handled by Identity
}