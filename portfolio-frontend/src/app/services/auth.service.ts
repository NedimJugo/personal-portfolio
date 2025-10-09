import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { LoginRequest } from '../models/auth/login-request.model';
import { LoginResponse } from '../models/auth/login-response.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { RegisterResponse } from '../models/auth/register-response.model';
import { RefreshTokenRequest } from '../models/auth/refresh-token-request.model';
import { RefreshTokenResponse } from '../models/auth/refresh-token-response.model';
import { LogoutRequest } from '../models/auth/logout-request.model';
import { UserInfo } from '../models/auth/user-info.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Initialize current user from stored token on service creation
    const token = this.getAccessToken();
    if (token) {
      console.log('AuthService initialized with existing token');
    }
  }

  /** LOGIN */
  login(request: LoginRequest): Observable<LoginResponse> {
    console.log('Login request sent');
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        console.log('Login response received, storing tokens');
        this.storeTokens(response);
        this.currentUserSubject.next(response.user);
        console.log('Tokens stored successfully:', {
          hasAccessToken: !!this.getAccessToken(),
          hasRefreshToken: !!this.getRefreshToken()
        });
      }),
      catchError((error: HttpErrorResponse) => {
        let message = 'Login failed. Please check your credentials.';
        
        if (error.status === 0) {
          message = 'Cannot connect to server. Please try again later.';
        } else if (error.status === 401) {
          message = 'Invalid email or password.';
        } else if (error.error?.message) {
          message = error.error.message;
        }

        return throwError(() => new Error(message));
      })
    );
  }

  /** REGISTER */
  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, request);
  }

  /** REFRESH TOKEN */
  refreshToken(request: RefreshTokenRequest): Observable<RefreshTokenResponse> {
    console.log('Refreshing token');
    return this.http.post<RefreshTokenResponse>(`${this.apiUrl}/refresh`, request).pipe(
      tap(response => {
        console.log('Token refreshed successfully');
        this.safeSetItem('accessToken', response.accessToken);
        this.safeSetItem('refreshToken', response.refreshToken);
      })
    );
  }

  /** LOGOUT */
  logout(refreshToken?: string): Observable<any> {
    const request: LogoutRequest = { refreshToken: refreshToken || '' };
    return this.http.post(`${this.apiUrl}/logout`, request).pipe(
      tap(() => {
        console.log('Logout successful, clearing session');
        this.clearSession();
      })
    );
  }

  /** GET CURRENT USER */
  getCurrentUser(): Observable<UserInfo> {
    console.log('Fetching current user');
    return this.http.get<UserInfo>(`${this.apiUrl}/me`).pipe(
      tap(user => {
        console.log('Current user fetched:', user.email);
        this.currentUserSubject.next(user);
      }),
      catchError((error) => {
        console.error('Error fetching current user:', error);
        return throwError(() => error);
      })
    );
  }

  /** REVOKE ALL TOKENS */
  revokeAllTokens(): Observable<any> {
    return this.http.post(`${this.apiUrl}/revoke-all-tokens`, {});
  }

  /** TOKEN MANAGEMENT */
  private storeTokens(response: LoginResponse) {
    console.log('Storing tokens in localStorage');
    this.safeSetItem('accessToken', response.accessToken);
    this.safeSetItem('refreshToken', response.refreshToken);
  }

  getAccessToken(): string | null {
    return this.safeGetItem('accessToken');
  }

  getRefreshToken(): string | null {
    return this.safeGetItem('refreshToken');
  }

  isLoggedIn(): boolean {
    const hasToken = !!this.getAccessToken();
    console.log('isLoggedIn check:', hasToken);
    return hasToken;
  }

  clearSession() {
    console.log('Clearing session');
    this.safeRemoveItem('accessToken');
    this.safeRemoveItem('refreshToken');
    this.currentUserSubject.next(null);
  }

  /** Optional: Add header with token */
  getAuthHeaders(): HttpHeaders {
    const token = this.getAccessToken();
    return new HttpHeaders({
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  /** ✅ Safe localStorage access helpers */
  private safeSetItem(key: string, value: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.setItem(key, value);
        console.log(`Token ${key} stored in localStorage`);
      } catch (error) {
        console.error(`Error storing ${key}:`, error);
      }
    }
  }

  private safeGetItem(key: string): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        return localStorage.getItem(key);
      } catch (error) {
        console.error(`Error retrieving ${key}:`, error);
        return null;
      }
    }
    return null;
  }

  private safeRemoveItem(key: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.removeItem(key);
        console.log(`Token ${key} removed from localStorage`);
      } catch (error) {
        console.error(`Error removing ${key}:`, error);
      }
    }
  }

  setTokens(accessToken: string, refreshToken: string): void {
    this.safeSetItem('accessToken', accessToken);
    this.safeSetItem('refreshToken', refreshToken);
  }

  clearTokens(): void {
    this.clearSession();
  }
}