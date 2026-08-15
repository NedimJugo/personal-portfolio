import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { SocialLinkResponse } from '../../models/social-link/social-link-response.model';
import { SocialLinkService } from '../../services/social-link.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SubscriberService } from '../../services/subscriber.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss'],
  providers: [SocialLinkService, SubscriberService]
})
export class FooterComponent implements OnInit {
  socialLinks$!: Observable<SocialLinkResponse[]>;
  currentYear = new Date().getFullYear();
  newsletterForm!: FormGroup;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';


  constructor(
    private socialLinkService: SocialLinkService,
    private subscriberService: SubscriberService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
  this.newsletterForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    website: [''] // honeypot - must stay empty
  });

  this.socialLinks$ = this.socialLinkService.get({ isVisible: true }).pipe(
    map((result) => result.items || []),
    catchError(() => of([]))
  );
}

  onSubscribe(): void {
    if (this.newsletterForm.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    const request = {
      email: this.newsletterForm.value.email,
      isActive: true,
      source: 'footer_newsletter',
      website: this.newsletterForm.value.website
    };

    this.subscriberService.create(request).subscribe({
      next: () => {
        this.successMessage = '🎉 Successfully subscribed!';
        this.newsletterForm.reset();
        this.isSubmitting = false;

        setTimeout(() => {
          this.successMessage = '';
        }, 5000);
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to subscribe. Please try again.';
        this.isSubmitting = false;

        setTimeout(() => {
          this.errorMessage = '';
        }, 5000);
      }
    });
  }
}