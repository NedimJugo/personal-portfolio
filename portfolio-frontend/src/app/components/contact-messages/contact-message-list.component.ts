import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgForm } from '@angular/forms';
import { BaseCrudComponent } from '../base/base-crud.component';
import { ContactMessageService } from '../../services/contact-message.service';
import { ContactMessageResponse } from '../../models/contact-message/contact-message-response.model';
import { ContactMessageSearchObject } from '../../models/contact-message/contact-message-search.model';
import { ContactMessageInsertRequest } from '../../models/contact-message/contact-message-insert-request.model';
import { ContactMessageUpdateRequest } from '../../models/contact-message/contact-message-update-request.model';

@Component({
  selector: 'app-contact-message-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact-message-list.component.html',
  styleUrls: ['./contact-message-list.component.scss']
})
export class ContactMessageListComponent extends BaseCrudComponent<
  ContactMessageResponse,
  ContactMessageSearchObject,
  ContactMessageInsertRequest,
  ContactMessageUpdateRequest,
  string
> {

  // Inject service using new inject() function
  private contactMessageService = inject(ContactMessageService);
  
  // Form models for create/edit operations
  createFormModel: ContactMessageInsertRequest = this.createEmptyInsertRequest();
  updateFormModel: ContactMessageUpdateRequest = {} as ContactMessageUpdateRequest;
  
  // Computed property to get the current form model
  get formModel(): ContactMessageInsertRequest | ContactMessageUpdateRequest {
    return this.showCreateModal ? this.createFormModel : this.updateFormModel;
  }

  // Expose Math for template
  Math = Math;

  constructor() {
    super(inject(ContactMessageService));
  }

  protected createDefaultSearch(): ContactMessageSearchObject {
    return {
      page: 0,
      pageSize: 20,
      sortBy: 'createdAt',
      desc: true,
      includeTotalCount: true
    };
  }

  protected createEmptyInsertRequest(): ContactMessageInsertRequest {
    return {
      name: '',
      email: '',
      subject: '',
      message: '',
      status: 'new',
      priority: 'medium'
    };
  }

  protected createUpdateRequestFromItem(item: ContactMessageResponse): ContactMessageUpdateRequest {
    return {
      subject: item.subject,
      message: item.message,
      phone: item.phone,
      company: item.company,
      projectType: item.projectType,
      budgetRange: item.budgetRange,
      source: item.source,
      status: item.status,
      priority: item.priority,
      handledById: item.handledById
    };
  }

  protected getItemId(item: ContactMessageResponse): string {
    return item.id;
  }

  // Override modal methods to handle form model
  override openCreateModal(): void {
    this.createFormModel = this.createEmptyInsertRequest();
    super.openCreateModal();
  }

  override openEditModal(item: ContactMessageResponse): void {
    this.updateFormModel = this.createUpdateRequestFromItem(item);
    super.openEditModal(item);
  }

  // Form submission handler
  onSubmitForm(form: NgForm): void {
    if (form.valid) {
      if (this.showCreateModal) {
        this.create(this.createFormModel);
      } else if (this.showEditModal && this.selectedItem) {
        this.update(this.selectedItem.id, this.updateFormModel);
      }
    }
  }

  // Specific methods for ContactMessage
  markAsHandled(item: ContactMessageResponse): void {
    const updateRequest: ContactMessageUpdateRequest = {
      ...this.createUpdateRequestFromItem(item),
      status: 'handled'
    };
    
    this.update(item.id, updateRequest);
  }

  filterByStatus(status: string): void {
    this.searchCriteria.status = status || undefined;
    this.onSearch();
  }

  // Pagination helper methods
  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getVisiblePages(): number[] {
    const totalPages = this.getTotalPages();
    const maxVisible = 5;
    const pages: number[] = [];
    
    if (totalPages <= maxVisible) {
      for (let i = 0; i < totalPages; i++) {
        pages.push(i);
      }
    } else {
      const half = Math.floor(maxVisible / 2);
      let start = Math.max(0, this.currentPage - half);
      let end = Math.min(totalPages - 1, start + maxVisible - 1);
      
      if (end - start < maxVisible - 1) {
        start = Math.max(0, end - maxVisible + 1);
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  // Track by function for ngFor performance
  trackByFn(index: number, item: ContactMessageResponse): string {
    return item.id;
  }
}
