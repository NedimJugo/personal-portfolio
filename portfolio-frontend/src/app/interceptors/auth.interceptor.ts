import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const accessToken = authService.getAccessToken();

  // Only skip token attachment for login, register, and refresh endpoints
  const skipAuth = req.url.includes('/auth/login') || 
                   req.url.includes('/auth/register') || 
                   req.url.includes('/auth/refresh');

  let clonedRequest = req;
  if (accessToken && !skipAuth) {
    clonedRequest = req.clone({
      setHeaders: { Authorization: `Bearer ${accessToken}` }
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 errors for authenticated endpoints
      if (error.status === 401 && 
          !skipAuth && 
          authService.getRefreshToken()) {
        
        const refreshRequest = {
          accessToken: authService.getAccessToken()!,
          refreshToken: authService.getRefreshToken()!
        };

        return authService.refreshToken(refreshRequest).pipe(
          switchMap((response) => {
            authService.setTokens(response.accessToken, response.refreshToken);
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${response.accessToken}` }
            });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            authService.clearTokens();
            router.navigate(['/admin/login']);
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};