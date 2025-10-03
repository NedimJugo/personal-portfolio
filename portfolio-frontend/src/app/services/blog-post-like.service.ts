// src/app/services/blog-post-like.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BlogPostLikeStatus } from '../models/blog-post-like/blog-post-like-status.model';

@Injectable({
  providedIn: 'root'
})
export class BlogPostLikeService {
  private baseUrl = `${environment.apiUrl}/blogpostlikes`;

  constructor(private http: HttpClient) {}

  toggleLike(blogPostId: string, visitorKey: string): Observable<BlogPostLikeStatus> {
    const params = new HttpParams()
      .set('blogPostId', blogPostId)
      .set('visitorKey', visitorKey);

    return this.http.post<BlogPostLikeStatus>(`${this.baseUrl}/toggle`, null, { params });
  }

  getLikeStatus(blogPostId: string, visitorKey: string): Observable<BlogPostLikeStatus> {
    const params = new HttpParams()
      .set('blogPostId', blogPostId)
      .set('visitorKey', visitorKey);

    return this.http.get<BlogPostLikeStatus>(`${this.baseUrl}/status`, { params });
  }
}