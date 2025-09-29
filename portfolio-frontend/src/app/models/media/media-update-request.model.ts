export interface MediaUpdateRequest {
  fileName?: string;
  originalFileName?: string;
  fileUrl?: string;
  storageProvider?: string;
  fileType?: string;
  fileSize?: number;
  mimeType?: string;
  width?: number;
  height?: number;
  altText?: string;
  caption?: string;
  folder?: string;
}
