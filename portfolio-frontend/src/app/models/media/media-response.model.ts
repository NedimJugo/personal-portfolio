export interface MediaResponse {
  id: string;               // Guid as string
  fileName: string;
  originalFileName: string;
  fileUrl: string;
  thumbnailUrl?: string;
  storageProvider: string;
  fileType: string;
  fileSize: number;
  mimeType: string;
  width?: number;
  height?: number;
  altText?: string;
  caption?: string;
  folder?: string;
  uploadedById: number;
  uploadedAt: string;       // ISO date string
}
