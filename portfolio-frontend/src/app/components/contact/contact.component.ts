import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { FooterComponent } from '../footer/footer.component';
import { SocialLinkResponse } from '../../models/social-link/social-link-response.model';
import { ContactMessageService } from '../../services/contact-message.service';
import { VisitorTrackingService } from '../../services/visitor-tracking.service';
import { SocialLinkService } from '../../services/social-link.service';
import { SiteContentService } from '../../services/site-content.service';
import { ContactMessageInsertRequest } from '../../models/contact-message/contact-message-insert-request.model';

type FormStatus = 'idle' | 'submitting' | 'success' | 'error';

interface QuickContactInfo {
  email: string;
  phone: string;
  location: string;
  responseTime: string;
  responseDescription: string;
}

const DEFAULT_CONTACT_INFO: QuickContactInfo = {
  email: 'hello@example.com',
  phone: '+1 (555) 123-4567',
  location: 'Sarajevo, Bosnia and Herzegovina',
  responseTime: '24-Hour Response Time',
  responseDescription: 'I typically respond within one business day!'
};

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavBarComponent, FooterComponent],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactComponent implements OnInit {
  contactForm!: FormGroup;
  formStatus = signal<FormStatus>('idle');
  errorMessage = signal<string>('');
  successMessage = signal<string>('');
  
  socialLinks$!: Observable<SocialLinkResponse[]>;
  contactInfo$!: Observable<QuickContactInfo>;
  
  readonly projectTypes = [
    'Web Application',
    'Mobile App',
    'E-commerce',
    'Portfolio/Blog',
    'API Development',
    'Consulting',
    'Other'
  ];
  
  readonly budgetRanges = [
    'Under 5,000KM',
    '5,000KM - 10,000KM',
    '10,000KM - 25,000KM',
    '25,000KM - 50,000KM',
    'Over 50,000KM',
    'Not sure yet'
  ];

  constructor(
    private fb: FormBuilder,
    private contactService: ContactMessageService,
    private visitorTrackingService: VisitorTrackingService,
    private socialLinkService: SocialLinkService,
    private siteContentService: SiteContentService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadSocialLinks();
    this.loadContactInfo();
    
    // Track contact page view
    this.visitorTrackingService.trackPageView('/contact').subscribe({
      next: () => console.log('Contact page view tracked'),
      error: (err) => console.error('Error tracking page view:', err)
    });
  }

  private loadSocialLinks(): void {
    this.socialLinks$ = this.socialLinkService.get({ 
      isVisible: true, 
      sortBy: 'displayOrder',
      pageSize: 10 
    }).pipe(
      map(result => result.items || []),
      catchError(error => {
        console.error('Error loading social links:', error);
        return of([]);
      })
    );
  }

  private loadContactInfo(): void {
    this.contactInfo$ = this.siteContentService.get({ 
      section: 'contact_info', 
      isPublished: true 
    }).pipe(
      map(result => {
        if (result.items && result.items.length > 0) {
          const contentItem = result.items.find(item => item.contentType === 'json');
          if (contentItem) {
            try {
              return JSON.parse(contentItem.content) as QuickContactInfo;
            } catch (error) {
              console.error('Error parsing contact info JSON:', error);
              return DEFAULT_CONTACT_INFO;
            }
          }
        }
        return DEFAULT_CONTACT_INFO;
      }),
      catchError(error => {
        console.error('Error loading contact info:', error);
        return of(DEFAULT_CONTACT_INFO);
      })
    );
  }

  private initializeForm(): void {
    this.contactForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      phone: ['', [Validators.maxLength(50)]],
      company: ['', [Validators.maxLength(200)]],
      subject: ['', [Validators.required, Validators.maxLength(300)]],
      message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]],
      projectType: [''],
      budgetRange: ['']
    });
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }

    this.formStatus.set('submitting');
    
    const formValue = this.contactForm.value;
    const request: ContactMessageInsertRequest = {
      ...formValue,
      userAgent: this.visitorTrackingService.getUserAgent(),
      source: 'website',
      status: 'new',
      priority: 'medium'
    };

    this.contactService.create(request).subscribe({
      next: (response) => {
        this.formStatus.set('success');
        this.successMessage.set(
          `Thank you for reaching out! Your message has been received and I'll get back to you within 24 hours. 🚀`
        );
        this.contactForm.reset();
      },
      error: (error) => {
        this.formStatus.set('error');
        this.errorMessage.set(
          error.message || 'Oops! Something went wrong. Please try again or email me directly.'
        );
      }
    });
  }

  closeModal(): void {
    this.formStatus.set('idle');
    this.successMessage.set('');
    this.errorMessage.set('');
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.contactForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  sanitizePhone(phone: string): string {
    return phone ? 'tel:' + phone.replace(/[^0-9+]/g, '') : '';
  }

  getFieldError(fieldName: string): string {
    const field = this.contactForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) return 'This field is required';
    if (field.errors['email']) return 'Please enter a valid email address';
    if (field.errors['minlength']) {
      return `Minimum ${field.errors['minlength'].requiredLength} characters required`;
    }
    if (field.errors['maxlength']) {
      return `Maximum ${field.errors['maxlength'].requiredLength} characters allowed`;
    }
    
    return 'Invalid value';
  }
}