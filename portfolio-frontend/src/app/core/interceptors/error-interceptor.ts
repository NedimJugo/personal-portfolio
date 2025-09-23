import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';
import { throwError, switchMap, take } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error) => {
      if (error.status === 401 && !req.url.includes('/auth/login')) {
        return authService.refreshToken().pipe(
          switchMap(() =>
            authService.token$.pipe(
              take(1),
              switchMap(token => {
                const newReq = req.clone({
                  setHeaders: { Authorization: `Bearer ${token}` }
                });
                return next(newReq);
              })
            )
          ),
          catchError(() => {
            authService.logout();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
