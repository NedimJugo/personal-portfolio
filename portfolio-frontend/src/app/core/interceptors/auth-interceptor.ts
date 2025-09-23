import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';
import { switchMap, take } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return authService.token$.pipe(
    take(1),
    switchMap(token => {
      if (token) {
        req = req.clone({
          setHeaders: { Authorization: `Bearer ${token}` }
        });
      }
      return next(req);
    })
  );
};
