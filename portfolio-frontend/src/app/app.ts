import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { VisitorTrackingService } from './services/visitor-tracking.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div class="app-container">
      <main class="container-fluid">
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