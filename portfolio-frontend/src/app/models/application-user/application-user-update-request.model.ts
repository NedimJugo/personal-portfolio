export interface ApplicationUserUpdateRequest {
  userName: string;
  email: string;
  fullName: string;
  isActive: boolean;
  password?: string; // optional update
}