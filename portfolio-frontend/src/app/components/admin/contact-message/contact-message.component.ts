import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip, forkJoin } from 'rxjs';
import { ContactMessageResponse } from '../../../models/contact-message/contact-message-response.model';
import { ContactMessageSearchObject } from '../../../models/contact-message/contact-message-search.model';
import { ContactMessageReplyResponse } from '../../../models/contact-message-reply/contact-message-reply-response.model';
import { ContactMessageService } from '../../../services/contact-message.service';
import { ContactMessageReplyService } from '../../../services/contact-message-reply.service';
import { ContactMessageReplySearchObject } from '../../../models/contact-message-reply/contact-message-reply-search.model';
import { ContactMessageReplyInsertRequest } from '../../../models/contact-message-reply/contact-message-reply-insert-request.model';
import { ContactMessageUpdateRequest } from '../../../models/contact-message/contact-message-update-request.model';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { EmailSyncService } from '../../../services/email-sync.service';


interface MessageStats {
  total: number;
  new: number;
  read: number;
  replied: number;
  archived: number;
  highPriority: number;
}

@Component({
  selector: 'app-contact-messages',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './contact-message.component.html',
  styleUrls: ['./contact-message.component.scss']
})
export class ContactMessagesComponent implements OnInit, OnDestroy {
  searchForm!: FormGroup;
  replyForm!: FormGroup;
  internalNoteForm!: FormGroup;

  private destroy$ = new Subject<void>();

  messages: ContactMessageResponse[] = [];
  totalCount: number | undefined;
  currentPage = 0;

  searchParams: ContactMessageSearchObject = {
    page: 0,
    pageSize: 20,
    sortBy: 'createdAt',
    desc: true,
    includeTotalCount: true,
    retrieveAll: false
  };

  // View state
  currentView: 'inbox' | 'detail' = 'inbox';
  selectedMessage: ContactMessageResponse | null = null;
  messageReplies: ContactMessageReplyResponse[] = [];

  isSyncing = false;
  
  // Loading states
  isLoading = false;
  isLoadingReplies = false;
  isSending = false;
  isUpdating = false;
  
  // Modal states
  showReplyModal = false;
  showInternalNoteModal = false;
  showDeleteModal = false;
  
  // Form errors
  replyError: string | null = null;
  noteError: string | null = null;
  
  // Stats
  stats: MessageStats = {
    total: 0,
    new: 0,
    read: 0,
    replied: 0,
    archived: 0,
    highPriority: 0
  };

  messageToDelete: ContactMessageResponse | null = null;
  isDeleting = false;

  constructor(
    private messageService: ContactMessageService,
    private replyService: ContactMessageReplyService,
    private fb: FormBuilder,
    private emailSyncService: EmailSyncService 
  ) {}

  ngOnInit(): void {
    this.initSearchForm();
    this.initReplyForm();
    this.initInternalNoteForm();
    this.loadMessages();
    this.loadStats();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initSearchForm(): void {
    this.searchForm = this.fb.group({
      name: [''],
      email: [''],
      subject: [''],
      status: [''],
      priority: [''],
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

  private initReplyForm(): void {
    this.replyForm = this.fb.group({
      replyToEmail: ['', [Validators.required, Validators.email]],
      subject: ['', Validators.required],
      replyMessage: ['', Validators.required],
      isInternal: [false]
    });
  }

  private initInternalNoteForm(): void {
    this.internalNoteForm = this.fb.group({
      replyMessage: ['', Validators.required]
    });
  }

  loadMessages(): void {
    this.isLoading = true;

    this.messageService.get(this.searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.messages = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => {
          console.error('Failed to load messages:', error);
        }
      });
  }

  loadStats(): void {
    const allMessagesSearch: ContactMessageSearchObject = {
      page: 0,
      pageSize: 1000,
      retrieveAll: true,
      includeTotalCount: true
    };

    this.messageService.get(allMessagesSearch)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          const messages = result.items;
          this.stats = {
            total: messages.length,
            new: messages.filter(m => m.status === 'new').length,
            read: messages.filter(m => m.status === 'read').length,
            replied: messages.filter(m => m.status === 'replied').length,
            archived: messages.filter(m => m.status === 'archived').length,
            highPriority: messages.filter(m => m.priority === 'high').length
          };
        },
        error: (error) => {
          console.error('Failed to load stats:', error);
        }
      });
  }

  private updateSearchParamsFromForm(): void {
    const formValue = this.searchForm.value;

    this.searchParams = {
      ...this.searchParams,
      name: formValue.name?.trim() || undefined,
      email: formValue.email?.trim() || undefined,
      subject: formValue.subject?.trim() || undefined,
      status: formValue.status || undefined,
      priority: formValue.priority || undefined,
      sortBy: formValue.sortBy || 'createdAt',
      desc: formValue.desc !== false,
      page: 0
    };
  }

  onSearchChange(): void {
    this.currentPage = 0;
    this.updateSearchParamsFromForm();
    this.loadMessages();
  }

  toggleSortOrder(): void {
    const currentDesc = this.searchForm.get('desc')?.value;
    this.searchForm.patchValue({ desc: !currentDesc });
  }

  hasActiveFilters(): boolean {
    const formValue = this.searchForm.value;
    return !!(
      formValue.name ||
      formValue.email ||
      formValue.subject ||
      formValue.status ||
      formValue.priority
    );
  }

  clearFilters(): void {
    this.searchForm.patchValue({
      name: '',
      email: '',
      subject: '',
      status: '',
      priority: '',
      sortBy: 'createdAt',
      desc: true
    });
  }

 syncEmails(): void {
  this.isSyncing = true;
  
  this.emailSyncService.syncEmails()
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isSyncing = false)
    )
    .subscribe({
      next: (response) => {
        console.log('Email sync completed:', response.message);
        this.loadMessages();
        this.loadStats();
      },
      error: (error) => {
        console.error('Email sync failed:', error);
      }
    });
}

  goToPage(page: number): void {
    this.currentPage = page;
    this.searchParams.page = page;
    this.loadMessages();
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

  openMessage(message: ContactMessageResponse): void {
    this.selectedMessage = message;
    this.currentView = 'detail';
    
    // Mark as read if it's new
    if (message.status === 'new') {
      this.updateMessageStatus(message.id, 'read');
    }

    this.loadMessageReplies(message.id);
  }

  backToInbox(): void {
    this.currentView = 'inbox';
    this.selectedMessage = null;
    this.messageReplies = [];
    this.loadMessages();
    this.loadStats();
  }

  loadMessageReplies(messageId: string): void {
    this.isLoadingReplies = true;

    const replySearch: ContactMessageReplySearchObject = {
      contactMessageId: messageId,
      sortBy: 'repliedAt',
      desc: false,
      retrieveAll: true
    };

    this.replyService.get(replySearch)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoadingReplies = false)
      )
      .subscribe({
        next: (result) => {
          this.messageReplies = result.items;
        },
        error: (error) => {
          console.error('Failed to load replies:', error);
        }
      });
  }

  openReplyModal(): void {
    if (!this.selectedMessage) return;

    this.replyForm.patchValue({
      replyToEmail: this.selectedMessage.email,
      subject: `Re: ${this.selectedMessage.subject}`,
      replyMessage: '',
      isInternal: false
    });

    this.replyError = null;
    this.showReplyModal = true;
  }

  closeReplyModal(): void {
    this.showReplyModal = false;
    this.replyForm.reset();
    this.replyError = null;
  }

  openInternalNoteModal(): void {
    this.internalNoteForm.reset();
    this.noteError = null;
    this.showInternalNoteModal = true;
  }

  closeInternalNoteModal(): void {
    this.showInternalNoteModal = false;
    this.internalNoteForm.reset();
    this.noteError = null;
  }

  submitReply(): void {
  if (this.replyForm.invalid || !this.selectedMessage) {
    this.replyForm.markAllAsTouched();
    return;
  }

  this.isSending = true;
  this.replyError = null;

  const formValue = this.replyForm.value;
  const payload: ContactMessageReplyInsertRequest = {
    contactMessageId: this.selectedMessage.id,  // ✅ Add this
    replyMessage: formValue.replyMessage,
    replyToEmail: formValue.replyToEmail,
    subject: formValue.subject,
    isInternal: formValue.isInternal || false
  };

  console.log('Sending payload:', JSON.stringify(payload, null, 2));

  this.replyService.create(payload)
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isSending = false)
    )
    .subscribe({
      next: (response) => {
        console.log('Reply successful:', response);
        if (!formValue.isInternal && this.selectedMessage) {
          this.updateMessageStatus(this.selectedMessage.id, 'replied');
        }
        
        this.closeReplyModal();
        if (this.selectedMessage) {
          this.loadMessageReplies(this.selectedMessage.id);
        }
      },
      error: (error) => {
        console.error('Reply failed with details:', error);
        this.replyError = error.message || 'Failed to send reply';
      }
    });
}

  submitInternalNote(): void {
  if (this.internalNoteForm.invalid || !this.selectedMessage) {
    this.internalNoteForm.markAllAsTouched();
    return;
  }

  this.isSending = true;
  this.noteError = null;

  const payload: ContactMessageReplyInsertRequest = {
    contactMessageId: this.selectedMessage.id,  // ✅ Add this
    replyMessage: this.internalNoteForm.value.replyMessage,
    isInternal: true
  };

  this.replyService.create(payload)
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isSending = false)
    )
    .subscribe({
      next: () => {
        this.closeInternalNoteModal();
        if (this.selectedMessage) {
          this.loadMessageReplies(this.selectedMessage.id);
        }
      },
      error: (error) => {
        this.noteError = error.message || 'Failed to save note';
        console.error('Internal note failed:', error);
      }
    });
}
 updateMessageStatus(messageId: string, status: string): void {
  this.isUpdating = true;

  if (!this.selectedMessage) {
    console.error('No message selected for status update');
    this.isUpdating = false;
    return;
  }

  // Create update payload with current message data plus the new status
  const updatePayload: ContactMessageUpdateRequest = {
    status,
    subject: this.selectedMessage.subject,
    message: this.selectedMessage.message,
    phone: this.selectedMessage.phone,
    company: this.selectedMessage.company,
    projectType: this.selectedMessage.projectType,
    budgetRange: this.selectedMessage.budgetRange,
    source: this.selectedMessage.source,
    priority: this.selectedMessage.priority
    // Don't include handledById unless you're actually changing it
  };

  console.log('Updating message status:', { messageId, status, updatePayload });

  this.messageService.update(messageId, updatePayload)
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isUpdating = false)
    )
    .subscribe({
      next: (updated) => {
        console.log('Status updated successfully:', updated);
        
        // Update local state
        const index = this.messages.findIndex(m => m.id === messageId);
        if (index !== -1) {
          this.messages[index] = { ...this.messages[index], status };
        }
        
        if (this.selectedMessage?.id === messageId) {
          this.selectedMessage = { ...this.selectedMessage, status };
        }
        
        this.loadStats();
      },
      error: (error) => {
        console.error('Failed to update status:', error);
        console.error('Error details:', {
          url: `api/contactmessages/${messageId}`,
          payload: updatePayload,
          error: error.message
        });
      }
    });
}

  updateMessagePriority(messageId: string, priority: string): void {
    this.isUpdating = true;

    const updatePayload: ContactMessageUpdateRequest = { priority };

    this.messageService.update(messageId, updatePayload)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isUpdating = false)
      )
      .subscribe({
        next: (updated) => {
          if (this.selectedMessage?.id === messageId) {
            this.selectedMessage = updated;
          }
          this.loadStats();
        },
        error: (error) => {
          console.error('Failed to update priority:', error);
        }
      });
  }

  quickMarkAsRead(message: ContactMessageResponse, event: Event): void {
    event.stopPropagation();
    this.updateMessageStatus(message.id, 'read');
    message.status = 'read';
  }

  quickSetPriority(message: ContactMessageResponse, priority: string, event: Event): void {
    event.stopPropagation();
    this.updateMessagePriority(message.id, priority);
    message.priority = priority;
  }

  quickArchive(message: ContactMessageResponse, event: Event): void {
    event.stopPropagation();
    this.updateMessageStatus(message.id, 'archived');
    message.status = 'archived';
  }

  confirmDelete(message: ContactMessageResponse): void {
    this.messageToDelete = message;
    this.showDeleteModal = true;
  }

  cancelDelete(): void {
    this.messageToDelete = null;
    this.showDeleteModal = false;
  }

  executeDelete(): void {
    if (!this.messageToDelete) return;

    this.isDeleting = true;

    this.messageService.delete(this.messageToDelete.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isDeleting = false)
      )
      .subscribe({
        next: () => {
          this.cancelDelete();
          if (this.currentView === 'detail' && this.selectedMessage?.id === this.messageToDelete?.id) {
            this.backToInbox();
          } else {
            this.loadMessages();
            this.loadStats();
          }
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
  }

  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getStatusClass(status: string): string {
    const classes: Record<string, string> = {
      'new': 'badge-primary',
      'read': 'badge-info',
      'replied': 'badge-success',
      'archived': 'badge-outline'
    };
    return classes[status] || 'badge-outline';
  }

  getPriorityClass(priority: string): string {
    const classes: Record<string, string> = {
      'high': 'badge-danger',
      'medium': 'badge-warning',
      'low': 'badge-info'
    };
    return classes[priority] || 'badge-outline';
  }

  formatDate(dateString: string | Date | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatRelativeTime(dateString: string | Date): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    
    return this.formatDate(date);
  }

  getUnreadCount(): number {
    return this.messages.filter(m => m.status === 'new').length;
  }
}