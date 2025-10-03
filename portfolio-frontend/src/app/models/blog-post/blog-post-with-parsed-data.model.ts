import { BlogPostLikeStatus } from "../blog-post-like/blog-post-like-status.model"
import { BlogPostResponse } from "./blog-post-response.model"

export interface BlogPostWithParsedData extends BlogPostResponse {
  parsedTags: string[]
  parsedCategories: string[]
  likeStatus?: BlogPostLikeStatus
}
