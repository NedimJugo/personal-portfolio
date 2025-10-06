import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { FooterComponent } from '../footer/footer.component';
import { SiteContentService } from '../../services/site-content.service';
import { EducationService } from '../../services/education.service';
import { CertificateService } from '../../services/certificate.service';
import { EducationResponse } from '../../models/education/education-response.model';
import { CertificateResponse } from '../../models/certificate/certificate-response.model';
import { MediaService } from '../../services/media.service';
import { EducationWithLogo } from '../../models/education/education-with-logo.model';
import { CertificateWithLogos } from '../../models/certificate/certificate-with-logo.model';
import { AboutContent } from '../../models/about/about-content.model';

const DEFAULT_ABOUT_CONTENT: AboutContent = {
  intro: "I'm a passionate developer with expertise in modern web technologies. I love creating efficient, scalable solutions that make a difference."
};

@Component({
  selector: 'app-about-page',
  standalone: true,
  imports: [CommonModule, NavBarComponent, FooterComponent],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AboutComponent implements OnInit {
  aboutContent$!: Observable<AboutContent>;
  educations$!: Observable<EducationWithLogo[]>;
  certificates$!: Observable<CertificateWithLogos[]>;

  readonly defaultLogo = 'https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg';

  constructor(
    private siteContentService: SiteContentService,
    private educationService: EducationService,
    private certificateService: CertificateService,
    private mediaService: MediaService
  ) {}

  ngOnInit(): void {
    this.aboutContent$ = this.loadAboutContent();
    this.educations$ = this.loadEducations();
    this.certificates$ = this.loadCertificates();
  }

  private loadAboutContent(): Observable<AboutContent> {
    return this.siteContentService.get({ 
      section: 'about_intro2', 
      isPublished: true 
    }).pipe(
      map((result) => {
        if (result.items && result.items.length > 0) {
          const aboutData = result.items[0];
          
          if (aboutData.contentType === 'html' || aboutData.contentType === 'text') {
            return { intro: aboutData.content };
          } else if (aboutData.contentType === 'json') {
            return JSON.parse(aboutData.content);
          }
        }
        return DEFAULT_ABOUT_CONTENT;
      }),
      catchError(() => of(DEFAULT_ABOUT_CONTENT))
    );
  }

  private loadEducations(): Observable<EducationWithLogo[]> {
    return this.educationService.get({ 
      sortBy: 'startDate', 
      desc: true 
    }).pipe(
      map((result) => result.items || []),
      switchMap((educations) => {
        if (educations.length === 0) return of([]);

        const educationsWithLogos$ = educations.map(edu => 
          this.loadEducationLogo(edu)
        );

        return Promise.all(educationsWithLogos$).then(results => results);
      }),
      catchError(() => of([]))
    );
  }

  private async loadEducationLogo(education: EducationResponse): Promise<EducationWithLogo> {
    if (!education.logoMediaId) {
      return { ...education, logoUrl: this.defaultLogo };
    }

    try {
      const media = await this.mediaService.getById(education.logoMediaId).toPromise();
      return { ...education, logoUrl: media?.fileUrl || this.defaultLogo };
    } catch {
      return { ...education, logoUrl: this.defaultLogo };
    }
  }

  private loadCertificates(): Observable<CertificateWithLogos[]> {
    return this.certificateService.get({ 
      isActive: true,
      isPublished: true,
      sortBy: 'issueDate', 
      desc: true 
    }).pipe(
      map((result) => result.items || []),
      switchMap((certificates) => {
        if (certificates.length === 0) return of([]);

        const certificatesWithLogos$ = certificates.map(cert => 
          this.loadCertificateMedia(cert)
        );

        return Promise.all(certificatesWithLogos$).then(results => results);
      }),
      catchError(() => of([]))
    );
  }

  private async loadCertificateMedia(certificate: CertificateResponse): Promise<CertificateWithLogos> {
    const result: CertificateWithLogos = { ...certificate };

    if (certificate.logoMediaId) {
      try {
        const logoMedia = await this.mediaService.getById(certificate.logoMediaId).toPromise();
        result.logoUrl = logoMedia?.fileUrl || this.defaultLogo;
      } catch {
        result.logoUrl = this.defaultLogo;
      }
    } else {
      result.logoUrl = this.defaultLogo;
    }

    if (certificate.certificateMediaId) {
      try {
        const certMedia = await this.mediaService.getById(certificate.certificateMediaId).toPromise();
        result.certificateUrl = certMedia?.fileUrl;
      } catch {
        // Certificate media optional
      }
    }

    return result;
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return 'Present';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short' });
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.defaultLogo;
  }
}