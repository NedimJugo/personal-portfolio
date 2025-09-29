import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BaseService } from './base.service';
import { BaseSearchObject } from '../../models/base/base-search-object.model';

@Injectable()
export abstract class BaseCrudService<
  TResponse,
  TSearch extends BaseSearchObject,
  TInsert,
  TUpdate,
  TId extends string | number
> extends BaseService<TResponse, TSearch, TId> {

  create(request: TInsert): Observable<TResponse> {
    return this.http.post<TResponse>(this.getFullUrl(), request)
      .pipe(
        catchError(this.handleError)
      );
  }

  update(id: TId, request: TUpdate): Observable<TResponse> {
    return this.http.put<TResponse>(`${this.getFullUrl()}/${id}`, request)
      .pipe(
        catchError(this.handleError)
      );
  }

  delete(id: TId): Observable<void> {
    return this.http.delete<void>(`${this.getFullUrl()}/${id}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  createBulk(requests: TInsert[]): Observable<TResponse[]> {
    return this.http.post<TResponse[]>(`${this.getFullUrl()}/bulk`, requests)
      .pipe(
        catchError(this.handleError)
      );
  }

  deleteBulk(ids: TId[]): Observable<void> {
    return this.http.delete<void>(`${this.getFullUrl()}/bulk`, { body: ids })
      .pipe(
        catchError(this.handleError)
      );
  }
}