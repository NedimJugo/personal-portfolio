// src/app/services/visitor-tracking.service.ts
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { PageViewInsertRequest } from '../models/page-view/page-view-insert-request.model';
import { PageViewResponse } from '../models/page-view/page-view-response.model';
import { environment } from '../../environments/environment';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class VisitorTrackingService {
  private readonly VISITOR_ID_KEY = 'visitorId';
  private readonly SESSION_TRACKED_KEY = 'sessionTracked'; // New: track session
  private baseUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  getOrCreateVisitorId(): string {
    if (isPlatformBrowser(this.platformId)) {
      let visitorId = localStorage.getItem(this.VISITOR_ID_KEY);

      if (!visitorId) {
        visitorId = this.generateVisitorId();
        localStorage.setItem(this.VISITOR_ID_KEY, visitorId);
      }

      return visitorId;
    }

    return this.generateVisitorId();
  }

  private generateVisitorId(): string {
    return `visitor_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  }

  /**
   * Tracks page view only once per session
   */
  trackPageView(path: string, projectId?: string, blogPostId?: string): Observable<PageViewResponse> {
    if (!isPlatformBrowser(this.platformId)) {
      return of({} as PageViewResponse);
    }

    // Check if already tracked in this session
    const sessionKey = `${this.SESSION_TRACKED_KEY}_${path}`;
    const alreadyTracked = sessionStorage.getItem(sessionKey);

    if (alreadyTracked) {
      console.log(`Page view already tracked for ${path} in this session`);
      return of({} as PageViewResponse);
    }

    const visitorKey = this.getOrCreateVisitorId();

    const request: PageViewInsertRequest = {
      path,
      referrer: document.referrer || undefined,
      userAgent: navigator.userAgent,
      visitorKey,  // Add this line
      ipAddress: undefined, // Backend will capture this from request
      country: undefined,    // Backend will resolve via IP
      city: undefined,       // Backend will resolve via IP
      projectId,
      blogPostId
    };

    return this.http.post<PageViewResponse>(`${this.baseUrl}/pageviews`, request).pipe(
      tap(() => {
        sessionStorage.setItem(sessionKey, 'true');
      })
    );
  }

  /**
   * Force track page view (bypasses session check) - useful for important events
   */
  forceTrackPageView(path: string, projectId?: string, blogPostId?: string): Observable<PageViewResponse> {
    if (!isPlatformBrowser(this.platformId)) {
      return of({} as PageViewResponse);
    }

    const visitorKey = this.getOrCreateVisitorId();

    const request: PageViewInsertRequest = {
      path,
      referrer: document.referrer || undefined,
      userAgent: navigator.userAgent,
      ipAddress: undefined,
      country: undefined,
      city: undefined,
      projectId,
      blogPostId
    };

    return this.http.post<PageViewResponse>(`${this.baseUrl}/pageviews`, request);
  }

  getUserAgent(): string {
    if (isPlatformBrowser(this.platformId)) {
      return navigator.userAgent;
    }
    return 'SSR-Unknown';
  }

  /**
   * Clear session tracking (useful for testing or when user logs out)
   */
  clearSessionTracking(): void {
    if (isPlatformBrowser(this.platformId)) {
      const keys = Object.keys(sessionStorage);
      keys.forEach(key => {
        if (key.startsWith(this.SESSION_TRACKED_KEY)) {
          sessionStorage.removeItem(key);
        }
      });
    }
  }
}