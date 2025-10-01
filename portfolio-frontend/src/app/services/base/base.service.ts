  import { Injectable } from '@angular/core';
  import { HttpClient, HttpParams } from '@angular/common/http';
  import { Observable, throwError } from 'rxjs';
  import { catchError, map } from 'rxjs/operators';
  import { environment } from '../../../environments/environment';
  import { BaseSearchObject } from '../../models/base/base-search-object.model';
  import { PagedResult } from '../../models/base/paged-result.model';

  @Injectable()
  export abstract class BaseService<
    TResponse,
    TSearch extends BaseSearchObject,
    TId extends string | number
  > {
    protected abstract endpoint: string;
    protected baseUrl = environment.apiUrl;

    constructor(protected http: HttpClient) {}

    protected getFullUrl(): string {
      return `${this.baseUrl}/${this.endpoint}`;
    }

    get(search?: TSearch): Observable<PagedResult<TResponse>> {
      let params = new HttpParams();
      
      if (search) {
        Object.keys(search).forEach(key => {
          const value = (search as any)[key];
          if (value !== undefined && value !== null && value !== '') {
            params = params.set(key, value.toString());
          }
        });
      }

      return this.http.get<PagedResult<TResponse>>(this.getFullUrl(), { params })
        .pipe(
          catchError(this.handleError)
        );
    }

    getById(id: TId): Observable<TResponse> {
      return this.http.get<TResponse>(`${this.getFullUrl()}/${id}`)
        .pipe(
          catchError(this.handleError)
        );
    }

    exists(id: TId): Observable<boolean> {
      return this.http.head(`${this.getFullUrl()}/${id}`)
        .pipe(
          map(() => true),
          catchError((error) => {
            if (error.status === 404) {
              return [false];
            }
            return throwError(error);
          })
        );
    }

    protected handleError(error: any): Observable<never> {
      console.error('API Error:', error);
      
      let errorMessage = 'An error occurred while processing your request.';
      
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      return throwError(() => new Error(errorMessage));
    }
  }