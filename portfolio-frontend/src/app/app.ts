import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

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
}