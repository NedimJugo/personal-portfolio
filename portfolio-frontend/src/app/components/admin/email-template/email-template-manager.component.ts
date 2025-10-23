import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip, forkJoin } from 'rxjs';
import { EmailTemplateResponse } from '../../../models/email-template/email-template-response.model';
import { EmailTemplateSearchObject } from '../../../models/email-template/email-template-search.model';
import { EmailTemplateService } from '../../../services/email-template.service';
import { SubscriberService } from '../../../services/subscriber.service';
import { EmailTemplateInsertRequest } from '../../../models/email-template/email-template-insert-request.model';
import { EmailTemplateUpdateRequest } from '../../../models/email-template/email-template-update-request.model';
import { SubscriberResponse } from '../../../models/subscriber/subscriber-response.model';
import { SanitizeHtmlPipe } from "../../../pipes/sanitize-html.pipe";


@Component({
  selector: 'app-email-template-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SanitizeHtmlPipe],
  templateUrl: './email-template-manager.component.html',
  styleUrls: ['./email-template-manager.component.scss']
})
export class EmailTemplateManagerComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  templateForm!: FormGroup;

  private destroy$ = new Subject<void>();

  templates: EmailTemplateResponse[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: EmailTemplateSearchObject = {
    page: 0,
    pageSize: 20,
    sortBy: 'createdAt',
    desc: true,
    includeTotalCount: true,
    retrieveAll: false
  };

  isLoading = false;
  isSaving = false;
  isDeleting = false;
  isSending = false;
  showModal = false;
  showDeleteModal = false;
  showSendModal = false;
  isEditMode = false;
  formError: string | null = null;

  currentTemplateId: string | null = null;
  templateToDelete: EmailTemplateResponse | null = null;
  templateToSend: EmailTemplateResponse | null = null;

  activeSubscribersCount = 0;
  sendingProgress = 0;

  constructor(
    private emailTemplateService: EmailTemplateService,
    private subscriberService: SubscriberService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.initSearchForm();
    this.initTemplateForm();
    this.loadTemplates();
    this.loadActiveSubscribersCount();

     this.templateForm.get('htmlContent')?.valueChanges
    .pipe(
      takeUntil(this.destroy$),
      debounceTime(500)
    )
    .subscribe(html => {
      if (html) {
        const plainText = this.extractPlainText(html);
        this.templateForm.patchValue({ textContent: plainText }, { emitEvent: false });
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      name: [''],
      isActive: [''],
      sortBy: ['createdAt'],
      desc: [true]
    });

    this.searchForm.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        skip(1)
      )
      .subscribe(() => {
        this.onSearchChange();
      });
  }

  private initTemplateForm(): void {
    this.templateForm = this.fb.group({
      name: ['', Validators.required],
      subject: ['', Validators.required],
      htmlContent: ['', Validators.required],
      textContent: ['', Validators.required],
      isActive: [true]
    });
  }

  loadTemplates(): void {
    this.isLoading = true;

    this.emailTemplateService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.templates = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => {
          console.error('Failed to load templates:', error);
          this.showNotification('Failed to load templates', 'error');
        }
      });
  }

  private loadActiveSubscribersCount(): void {
    this.subscriberService.get({
      isActive: true,
      pageSize: 1,
      includeTotalCount: true
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.activeSubscribersCount = result.totalCount || 0;
        },
        error: (error) => {
          console.error('Failed to load subscribers count:', error);
        }
      });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    let isActiveValue: boolean | undefined;
    if (formValue.isActive === true || formValue.isActive === 'true') {
      isActiveValue = true;
    } else if (formValue.isActive === false || formValue.isActive === 'false') {
      isActiveValue = false;
    } else {
      isActiveValue = undefined;
    }

    this.searchParams = {
      ...this.searchParams,
      name: formValue.name?.trim() || undefined,
      isActive: isActiveValue,
      sortBy: formValue.sortBy || 'createdAt',
      desc: formValue.desc || false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadTemplates();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(formValue.name || formValue.isActive !== '');
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      name: '',
      isActive: '',
      sortBy: 'createdAt',
      desc: true
    });
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadTemplates();
  }

  getTotalPages(): number {
    if (!this.totalCount) return 1;
    return Math.ceil(this.totalCount / (this.searchParams.pageSize || 20));
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(0, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(totalPages, start + maxVisible);

    if (end - start < maxVisible) {
      start = Math.max(0, end - maxVisible);
    }

    for (let i = start; i < end; i++) {
      pages.push(i);
    }

    return pages;
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.currentTemplateId = null;
    this.formError = null;
    this.templateForm.reset({
      isActive: true
    });
    this.showModal = true;
  }

  openEditModal(template: EmailTemplateResponse): void {
    this.isEditMode = true;
    this.currentTemplateId = template.id;
    this.formError = null;

    this.templateForm.patchValue({
      name: template.name,
      subject: template.subject,
      htmlContent: template.htmlContent,
      textContent: template.textContent,
      isActive: template.isActive
    });

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.templateForm.reset();
    this.currentTemplateId = null;
    this.formError = null;
  }

  submitForm(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.formError = null;

    const formValue = this.templateForm.value;
    const payload = { ...formValue };

    const request$ = this.isEditMode && this.currentTemplateId
      ? this.emailTemplateService.update(this.currentTemplateId, payload as EmailTemplateUpdateRequest)
      : this.emailTemplateService.create(payload as EmailTemplateInsertRequest);

    request$
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSaving = false)
      )
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadTemplates();
          this.showNotification(
            this.isEditMode ? 'Template updated successfully' : 'Template created successfully',
            'success'
          );
        },
        error: (error) => {
          this.formError = error.message || 'An error occurred while saving';
          console.error('Save failed:', error);
        }
      });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.templateForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  confirmDelete(template: EmailTemplateResponse): void {
    this.templateToDelete = template;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.templateToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.templateToDelete) return;

    this.isDeleting = true;

    this.emailTemplateService.delete(this.templateToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          this.loadTemplates();
          this.showNotification('Template deleted successfully', 'success');
        },
        error: (error) => {
          console.error('Delete failed:', error);
          this.showNotification('Failed to delete template', 'error');
        }
      });
  }

  toggleTemplateActive(template: EmailTemplateResponse, event: Event): void {
    event.stopPropagation();

    const updatePayload: EmailTemplateUpdateRequest = {
      isActive: !template.isActive
    };

    this.emailTemplateService.update(template.id, updatePayload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          template.isActive = !template.isActive;
          this.showNotification('Template status updated', 'success');
        },
        error: (error) => {
          console.error('Failed to toggle active status:', error);
          this.showNotification('Failed to update template status', 'error');
        }
      });
  }

  confirmSend(template: EmailTemplateResponse): void {
    this.templateToSend = template;
    this.showSendModal = true;
  }

  cancelSend(): void {
    this.templateToSend = null;
    this.showSendModal = false;
    this.sendingProgress = 0;
  }

  executeSend(): void {
    if (!this.templateToSend) return;

    this.isSending = true;
    this.sendingProgress = 0;

    // Get all active subscribers
    this.subscriberService.get({
      isActive: true,
      pageSize: 1000,
      retrieveAll: true
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isSending = false;
          this.cancelSend();
        })
      )
      .subscribe({
        next: (result) => {
          const subscribers = result.items;

          if (subscribers.length === 0) {
            this.showNotification('No active subscribers found', 'warning');
            return;
          }

          this.sendEmailsToSubscribers(subscribers, this.templateToSend!);
        },
        error: (error) => {
          console.error('Failed to load subscribers:', error);
          this.showNotification('Failed to load subscribers', 'error');
        }
      });
  }

  private sendEmailsToSubscribers(subscribers: SubscriberResponse[], template: EmailTemplateResponse): void {
    const emails = subscribers.map(s => s.email);

    this.emailTemplateService.sendToSubscribers(template.id, emails)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isSending = false;
          this.cancelSend();
        })
      )
      .subscribe({
        next: (result) => {
          this.showNotification(
            `Emails sent: ${result.totalSent} successful, ${result.totalFailed} failed`,
            result.totalFailed === 0 ? 'success' : 'warning'
          );
        },
        error: (error) => {
          console.error('Failed to send emails:', error);
          this.showNotification('Failed to send emails', 'error');
        }
      });
  }

  private extractPlainText(html: string): string {
    // Create a temporary div element
    const temp = document.createElement('div');
    temp.innerHTML = html;

    // Get text content and clean it up
    let text = temp.textContent || temp.innerText || '';

    // Remove extra whitespace and normalize line breaks
    text = text.replace(/\s+/g, ' ').trim();

    return text;
  }


  getPreviewHtml(): string {
    return this.templateForm.get('htmlContent')?.value || '<p>No content to preview</p>';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  private showNotification(message: string, type: 'success' | 'error' | 'warning'): void {
    // Simple notification implementation
    // In a real app, you might use a proper notification service or snackbar
    console.log(`[${type.toUpperCase()}] ${message}`);
    alert(message);
  }
}