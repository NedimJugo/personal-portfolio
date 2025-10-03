import { BlogPostLikeStatus } from "../blog-post-like/blog-post-like-status.model";
import { BlogPostResponse } from "./blog-post-response.model";

export interface BlogPostWithLikeStatus extends BlogPostResponse {
  likeStatus?: BlogPostLikeStatus
}
