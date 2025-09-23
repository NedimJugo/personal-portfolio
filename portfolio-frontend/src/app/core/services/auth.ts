import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RefreshTokenRequest, User } from '../models/auth-response';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private isBrowser: boolean;

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    // Check for existing token on service initialization
    this.loadTokenFromStorage();
  }

  private loadTokenFromStorage(): void {
    // Only access localStorage in the browser
    if (!this.isBrowser) {
      return;
    }

    const token = localStorage.getItem('access_token');
    const refreshToken = localStorage.getItem('refresh_token');
    
    if (token && this.isTokenValid(token)) {
      this.tokenSubject.next(token);
      const user = this.getUserFromToken(token);
      this.currentUserSubject.next(user);
    } else if (refreshToken) {
      this.refreshToken().subscribe({
        error: () => this.logout()
      });
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}${environment.apiEndpoints.auth.login}`,
      credentials
    ).pipe(
      tap(response => {
        this.setSession(response);
      }),
      catchError(error => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    if (!this.isBrowser) {
      return throwError(() => new Error('Cannot refresh token on server'));
    }

    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const request: RefreshTokenRequest = { refreshToken };
    
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}${environment.apiEndpoints.auth.refresh}`,
      request
    ).pipe(
      tap(response => {
        this.setSession(response);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    if (!this.isBrowser) {
      return;
    }

    const refreshToken = localStorage.getItem('refresh_token');
    
    // Call logout endpoint if refresh token exists
    if (refreshToken) {
      this.http.post(
        `${environment.apiUrl}${environment.apiEndpoints.auth.logout}`,
        { refreshToken }
      ).subscribe();
    }

    // Clear local storage and subjects
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    this.currentUserSubject.next(null);
    this.tokenSubject.next(null);
    
    this.router.navigate(['/login']);
  }

  private setSession(authResponse: AuthResponse): void {
    if (!this.isBrowser) {
      return;
    }

    localStorage.setItem('access_token', authResponse.token);
    localStorage.setItem('refresh_token', authResponse.refreshToken);
    
    this.tokenSubject.next(authResponse.token);
    this.currentUserSubject.next(authResponse.user);
  }

  private isTokenValid(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp > currentTime;
    } catch {
      return false;
    }
  }

  private getUserFromToken(token: string): User {
    const decoded: any = jwtDecode(token);
    return {
      id: decoded.sub,
      email: decoded.email,
      fullName: decoded.name,
      isActive: true,
      roles: decoded.role || []
    };
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.currentUserValue;
  }

  get isAdmin(): boolean {
    const user = this.currentUserValue;
    return user?.roles?.includes('Admin') || false;
  }

  // Helper method to get token safely
  getToken(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem('access_token');
  }

  // Helper method to get refresh token safely
  getRefreshToken(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem('refresh_token');
  }
}