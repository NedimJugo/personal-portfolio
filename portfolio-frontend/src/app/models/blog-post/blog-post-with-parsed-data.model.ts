import { BlogPostResponse } from "./blog-post-response.model"

export interface BlogPostWithParsedData extends BlogPostResponse {
  parsedTags: string[]
  parsedCategories: string[]
}
