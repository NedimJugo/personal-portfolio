import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Check if running in the browser before accessing localStorage
  let authToken: string | null = null;

  if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
    authToken = localStorage.getItem('authToken');
  }

  if (authToken) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`
      }
    });
  }

  return next(req);
};
