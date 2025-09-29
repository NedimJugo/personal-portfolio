export interface ProjectImageInsertRequest {
  mediaId: string;    // Guid as string
  caption: string;
  order: number;
  isHero: boolean;
}
