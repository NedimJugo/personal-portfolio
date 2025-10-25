import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { Router } from "@angular/router"
import { forkJoin, type Observable, of } from "rxjs"
import { catchError, map, switchMap } from "rxjs/operators"
import { NavBarComponent } from "../nav-bar/nav-bar.component"
import { FooterComponent } from "../footer/footer.component"
import { ProjectService } from "../../services/project.service"
import { ProjectImageService } from "../../services/project-image.service"
import { MediaService } from "../../services/media.service"
import { ProjectWithImage } from "../../models/project-image/project-with-image.model"
import { ProjectResponse } from "../../models/project/project-response.model"
import { VisitorTrackingService } from "../../services/visitor-tracking.service"

@Component({
  selector: "app-projects",
  standalone: true,
  imports: [CommonModule, FooterComponent, NavBarComponent],
  templateUrl: "./projects.component.html",
  styleUrls: ["./projects.component.scss"],
})
export class ProjectsComponent implements OnInit {
  projects$!: Observable<ProjectWithImage[]>
  
  readonly defaultProjectImage = 'https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg'

  constructor(
    private projectService: ProjectService,
    private projectImageService: ProjectImageService,
    private mediaService: MediaService,
    private router: Router,
    private visitorTrackingService: VisitorTrackingService
  ) {}

  ngOnInit(): void {
    this.visitorTrackingService.trackPageView('/projects').subscribe({
      next: () => console.log('Projects page view tracked'),
      error: (err) => console.error('Error tracking page view:', err)
    })
    this.projects$ = this.loadProjectsWithImages()
  }

  private loadProjectsWithImages(): Observable<ProjectWithImage[]> {
    return this.projectService.get({ 
  isPublished: true, 
  pageSize: 100,
  sortBy: 'displayOrder',
  desc: false
}).pipe(
      switchMap((result) => {
        const projects = result.items || []
        
        if (projects.length === 0) {
          return of([])
        }

        const projectsWithImages$ = projects.map(project => 
          this.loadProjectHeroImage(project)
        )

        return forkJoin(projectsWithImages$)
      }),
      catchError((error) => {
        console.error('Error loading projects with images:', error)
        return of([])
      })
    )
  }

 private loadProjectHeroImage(project: ProjectResponse): Observable<ProjectWithImage> {
  // First try to use featuredMediaId if it exists
  if (project.featuredMediaId) {
    return this.mediaService.getById(project.featuredMediaId).pipe(
      map((media) => ({
        ...project,
        heroImageUrl: media.fileUrl,
        heroImageAlt: media.altText || project.title
      } as ProjectWithImage)),
      catchError(() => this.getFallbackImage(project))
    )
  }

  // Otherwise try to find a hero image via projectImages
  return this.projectImageService.get({ 
    projectId: project.id, 
    isHero: true,
    pageSize: 1 
  }).pipe(
    switchMap((imageResult) => {
      const projectImage = imageResult.items?.[0]
      
      if (!projectImage) {
        return this.getFallbackImage(project)
      }

      return this.mediaService.getById(projectImage.mediaId).pipe(
        map((media) => ({
          ...project,
          heroImageUrl: media.fileUrl,
          heroImageAlt: media.altText || projectImage.caption || project.title
        } as ProjectWithImage)),
        catchError(() => this.getFallbackImage(project))
      )
    }),
    catchError(() => this.getFallbackImage(project))
  )
}

private getFallbackImage(project: ProjectResponse): Observable<ProjectWithImage> {
  return of({
    ...project,
    heroImageUrl: this.defaultProjectImage,
    heroImageAlt: project.title
  } as ProjectWithImage)
}

  viewProjectDetails(projectId: string): void {
    this.router.navigate(['/projects', projectId])
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement
    img.src = this.defaultProjectImage
  }
}