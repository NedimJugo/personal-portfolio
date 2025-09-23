import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterModule } from '@angular/router';
import { ProjectService } from '../../core/services/project';
import { Project } from '../../core/models/project';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, 
    MatButtonModule, 
    MatCardModule, 
    RouterModule
  ],
  template: `
    <div class="home-container">
      <!-- Hero Section -->
      <section class="hero-section">
        <div class="hero-content">
          <h1 class="hero-title">Full Stack Developer</h1>
          <p class="hero-subtitle">
            Building modern web applications with Angular, ASP.NET Core, and cloud technologies
          </p>
          <div class="hero-actions">
            <button mat-raised-button color="primary" routerLink="/projects">
              View My Work
            </button>
            <button mat-stroked-button routerLink="/contact">
              Get In Touch
            </button>
          </div>
        </div>
      </section>

      <!-- Featured Projects -->
      <section class="featured-projects">
        <div class="container">
          <h2>Featured Projects</h2>
          <div class="projects-grid" *ngIf="featuredProjects.length > 0; else loadingTemplate">
            <mat-card *ngFor="let project of featuredProjects" class="project-card">
              <mat-card-header>
                <mat-card-title>{{ project.title }}</mat-card-title>
              </mat-card-header>
              <img 
                mat-card-image 
                [src]="project.featuredMediaUrl || '/assets/images/placeholder.jpg'" 
                [alt]="project.title"
                *ngIf="project.featuredMediaUrl"
              >
              <mat-card-content>
                <p>{{ project.shortDescription }}</p>
                <div class="tech-tags">
                  <span 
                    *ngFor="let tech of project.techs" 
                    class="tech-tag"
                  >
                    {{ tech.name }}
                  </span>
                </div>
              </mat-card-content>
              <mat-card-actions>
                <button 
                  mat-button 
                  color="primary"
                  [routerLink]="['/projects', project.slug]"
                >
                  View Details
                </button>
                <a 
  mat-button 
  *ngIf="project.liveUrl"
  [href]="project.liveUrl"
  target="_blank"
  rel="noopener noreferrer"
>
  Live Demo
</a>
              </mat-card-actions>
            </mat-card>
          </div>
          
          <ng-template #loadingTemplate>
            <div class="loading-state">
              <p>Loading featured projects...</p>
            </div>
          </ng-template>
        </div>
      </section>

      <!-- Skills Overview -->
      <section class="skills-section">
        <div class="container">
          <h2>Technical Skills</h2>
          <div class="skills-grid">
            <div class="skill-category">
              <h3>Frontend</h3>
              <ul>
                <li>Angular 17+</li>
                <li>TypeScript</li>
                <li>RxJS</li>
                <li>Angular Material</li>
                <li>SCSS/CSS3</li>
              </ul>
            </div>
            <div class="skill-category">
              <h3>Backend</h3>
              <ul>
                <li>ASP.NET Core</li>
                <li>Entity Framework</li>
                <li>SQL Server</li>
                <li>RESTful APIs</li>
                <li>JWT Authentication</li>
              </ul>
            </div>
            <div class="skill-category">
              <h3>DevOps & Tools</h3>
              <ul>
                <li>Azure</li>
                <li>Docker</li>
                <li>Git</li>
                <li>CI/CD</li>
                <li>Visual Studio</li>
              </ul>
            </div>
          </div>
        </div>
      </section>
    </div>
  `,
  styleUrls: ['./home.scss']
})
export class Home implements OnInit {
  featuredProjects: Project[] = [];
  loading = true;

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    this.loadFeaturedProjects();
  }

  private loadFeaturedProjects(): void {
    this.projectService.getProjects({ pageSize: 3 }).subscribe({
      next: (response) => {
        this.featuredProjects = response.data.slice(0, 3);
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load featured projects:', error);
        this.loading = false;
      }
    });
  }
}