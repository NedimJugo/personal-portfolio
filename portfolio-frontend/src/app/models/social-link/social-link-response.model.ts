export interface SocialLinkResponse {
  id: string;
  platform: string;
  displayName: string;
  url: string;
  iconClass?: string;
  color?: string;
  isVisible: boolean;
  displayOrder: number;
  createdAt: Date;
  updatedAt: Date;
}