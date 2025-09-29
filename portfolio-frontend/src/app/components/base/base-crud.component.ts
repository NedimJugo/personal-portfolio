import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize } from 'rxjs';
import { BaseListComponent } from './base-list.component';
import { BaseCrudService } from '../../services/base/base-crud.service';
import { BaseSearchObject } from '../../models/base/base-search-object.model';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: ''
})
export abstract class BaseCrudComponent<
  TResponse,
  TSearch extends BaseSearchObject,
  TInsert,
  TUpdate,
  TId extends string | number
> extends BaseListComponent<TResponse, TSearch, TId> {

  // CRUD operations state
  creating = false;
  updating = false;
  deleting = false;
  
  // Selected items
  selectedItems: TResponse[] = [];
  selectedItem: TResponse | null = null;
  
  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;

  protected crudService: BaseCrudService<TResponse, TSearch, TInsert, TUpdate, TId>;

  constructor(service: BaseCrudService<TResponse, TSearch, TInsert, TUpdate, TId>) {
    super(service);
    this.crudService = service;
  }

  protected abstract createEmptyInsertRequest(): TInsert;
  protected abstract createUpdateRequestFromItem(item: TResponse): TUpdate;
  protected abstract getItemId(item: TResponse): TId;

  // Create operations
  openCreateModal(): void {
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  create(request: TInsert): void {
    this.creating = true;
    this.error = null;

    this.crudService.create(request)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.creating = false)
      )
      .subscribe({
        next: () => {
          this.closeCreateModal();
          this.loadItems();
        },
        error: (error: Error) => {
          this.error = error.message;
        }
      });
  }

  // Edit operations
  openEditModal(item: TResponse): void {
    this.selectedItem = item;
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedItem = null;
  }

  update(id: TId, request: TUpdate): void {
    this.updating = true;
    this.error = null;

    this.crudService.update(id, request)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.updating = false)
      )
      .subscribe({
        next: () => {
          this.closeEditModal();
          this.loadItems();
        },
        error: (error: Error) => {
          this.error = error.message;
        }
      });
  }

  // Delete operations
  openDeleteModal(item: TResponse): void {
    this.selectedItem = item;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedItem = null;
  }

  delete(id: TId): void {
    this.deleting = true;
    this.error = null;

    this.crudService.delete(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.deleting = false)
      )
      .subscribe({
        next: () => {
          this.closeDeleteModal();
          this.loadItems();
        },
        error: (error: Error) => {
          this.error = error.message;
        }
      });
  }

  // Selection
  toggleSelection(item: TResponse): void {
    const index = this.selectedItems.indexOf(item);
    if (index > -1) {
      this.selectedItems.splice(index, 1);
    } else {
      this.selectedItems.push(item);
    }
  }

  selectAll(): void {
    this.selectedItems = [...this.items];
  }

  clearSelection(): void {
    this.selectedItems = [];
  }

  // Bulk operations
  deleteBulk(): void {
    if (this.selectedItems.length === 0) return;

    this.deleting = true;
    this.error = null;
    
    const ids = this.selectedItems.map(item => this.getItemId(item));

    this.crudService.deleteBulk(ids)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.deleting = false)
      )
      .subscribe({
        next: () => {
          this.clearSelection();
          this.loadItems();
        },
        error: (error: Error) => {
          this.error = error.message;
        }
      });
  }
}