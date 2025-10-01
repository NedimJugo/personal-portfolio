import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { SocialLinkResponse } from '../../models/social-link/social-link-response.model';
import { SocialLinkService } from '../../services/social-link.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {
  socialLinks$!: Observable<SocialLinkResponse[]>;
  currentYear = new Date().getFullYear();

  constructor(private socialLinkService: SocialLinkService) {}

  ngOnInit(): void {
    this.socialLinks$ = this.socialLinkService.get({ isVisible: true }).pipe(
      map((result) => result.items || []),
      catchError(() => of([]))
    );
  }
}