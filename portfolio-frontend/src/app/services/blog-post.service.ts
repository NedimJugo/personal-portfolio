import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseCrudService } from './base/base-crud.service';
import { BlogPostResponse } from '../models/blog-post/blog-post-response.model';
import { BlogPostSearchObject } from '../models/blog-post/blog-post-search.model';
import { BlogPostInsertRequest } from '../models/blog-post/blog-post-insert-request.model';
import { BlogPostUpdateRequest } from '../models/blog-post/blog-post-update-request.model';

@Injectable({
  providedIn: 'root'
})
export class BlogPostService extends BaseCrudService<
  BlogPostResponse,
  BlogPostSearchObject,
  BlogPostInsertRequest,
  BlogPostUpdateRequest,
  string
> {
  
  protected endpoint = 'blogposts';

  constructor(http: HttpClient) {
    super(http);
  }
}