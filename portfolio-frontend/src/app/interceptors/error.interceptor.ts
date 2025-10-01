import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: any) => {
      let errorMessage = 'An unexpected error occurred';

      // Check if we're in a browser environment before using ErrorEvent
      const isBrowser = typeof window !== 'undefined' && typeof ErrorEvent !== 'undefined';
      
      if (isBrowser && error.error instanceof ErrorEvent) {
        // Client-side error (network error, etc.)
        errorMessage = `Client error: ${error.error.message}`;
      } else {
        // Server-side error or non-browser environment
        switch (error.status) {
          case 0:
            // Network error (e.g., CORS, offline)
            errorMessage = 'Unable to connect to the server. Please check your internet connection.';
            break;
          case 400:
            errorMessage = error.error?.message || 'Bad request. Please check your input.';
            break;
          case 401:
            // Unauthorized - redirect to login
            if (router) {
              router.navigate(['/login']);
            }
            errorMessage = 'Unauthorized access. Please log in.';
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = 'The requested resource was not found.';
            break;
          case 409:
            errorMessage = error.error?.message || 'Conflict occurred.';
            break;
          case 422:
            errorMessage = error.error?.message || 'Validation failed.';
            break;
          case 500:
            errorMessage = 'Internal server error. Please try again later.';
            break;
          case 503:
            errorMessage = 'Service temporarily unavailable. Please try again later.';
            break;
          default:
            errorMessage = error.error?.message || 
                          error.message || 
                          `Error ${error.status}: ${error.statusText || 'Unknown error'}`;
        }
      }

      console.error('HTTP Error Interceptor:', {
        url: req.url,
        method: req.method,
        status: error.status,
        message: errorMessage,
        error: error
      });

      // Create a proper error object with additional context
      const enhancedError = new Error(errorMessage);
      (enhancedError as any).originalError = error;
      (enhancedError as any).status = error.status;
      (enhancedError as any).url = req.url;

      return throwError(() => enhancedError);
    })
  );
};