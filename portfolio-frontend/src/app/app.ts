import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { VisitorTrackingService } from './services/visitor-tracking.service';
import { WarmupLoaderComponent } from './components/warmup-loader/warmup-loader.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, WarmupLoaderComponent],
  template: `
    <div class="app-container">
      <main class="container-fluid">
        <app-warmup-loader></app-warmup-loader>
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styleUrls: ['./app.scss']
})
export class AppComponent {
  title = 'portfolio-admin';

  constructor(private visitorTrackingService: VisitorTrackingService) {}

  ngOnInit(): void {
    // Initialize visitor ID on app load
    this.visitorTrackingService.getOrCreateVisitorId();
  }
}