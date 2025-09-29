export interface SiteContentResponse {
  id: string;
  section: string;
  contentType: 'text' | 'html' | 'json';
  content: string;
  isPublished: boolean;
  createdAt: Date;
  updatedAt: Date;
}