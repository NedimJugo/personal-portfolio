import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize } from 'rxjs';
import { BaseService } from '../../services/base/base.service';
import { BaseSearchObject } from '../../models/base/base-search-object.model';
import { PagedResult } from '../../models/base/paged-result.model';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: ''
})
export abstract class BaseListComponent<
  TResponse,
  TSearch extends BaseSearchObject,
  TId extends string | number
> implements OnInit, OnDestroy {
  
  protected destroy$ = new Subject<void>();
  
  items: TResponse[] = [];
  totalCount = 0;
  loading = false;
  error: string | null = null;
  
  // Pagination
  currentPage = 0;
  pageSize = 20;
  
  // Search
  searchCriteria: TSearch = this.createDefaultSearch();
  
  // Sorting
  sortBy = 'id';
  sortDesc = false;

  constructor(protected service: BaseService<TResponse, TSearch, TId>) {}

  ngOnInit(): void {
    this.loadItems();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected abstract createDefaultSearch(): TSearch;

  loadItems(): void {
    this.loading = true;
    this.error = null;
    
    const search: TSearch = {
      ...this.searchCriteria,
      page: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      desc: this.sortDesc,
      includeTotalCount: true
    };

    this.service.get(search)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (result: PagedResult<TResponse>) => {
          this.items = result.items;
          this.totalCount = result.totalCount || 0;
        },
        error: (error: Error) => {
          this.error = error.message;
          this.items = [];
          this.totalCount = 0;
        }
      });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadItems();
  }

  onPageSizeChange(pageSize: number): void {
    this.pageSize = pageSize;
    this.currentPage = 0;
    this.loadItems();
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDesc = !this.sortDesc;
    } else {
      this.sortBy = column;
      this.sortDesc = false;
    }
    this.currentPage = 0;
    this.loadItems();
  }

  onSearch(): void {
    this.currentPage = 0;
    this.loadItems();
  }

  onReset(): void {
    this.searchCriteria = this.createDefaultSearch();
    this.currentPage = 0;
    this.sortBy = 'id';
    this.sortDesc = false;
    this.loadItems();
  }

  refresh(): void {
    this.loadItems();
  }
}