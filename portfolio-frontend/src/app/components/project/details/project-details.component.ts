import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { ActivatedRoute, Router } from "@angular/router"
import { forkJoin, type Observable, of } from "rxjs"
import { catchError, map, switchMap, startWith } from "rxjs/operators"
import { ProjectDetails } from "../../../models/project/project-details.model"
import { ProjectService } from "../../../services/project.service"
import { ProjectImageService } from "../../../services/project-image.service"
import { MediaService } from "../../../services/media.service"
import { ProjectTagService } from "../../../services/project-tag.service"
import { ProjectTechService } from "../../../services/project-tech.service"
import { TagService } from "../../../services/tag.service"
import { TechService } from "../../../services/tech.service"
import { NavBarComponent } from "../../nav-bar/nav-bar.component";
import { FooterComponent } from "../../footer/footer.component";
import { TagResponse } from "../../../models/tag/tag-response.model"
import { TechResponse } from "../../../models/tech/tech-response.model"
import { ProjectImage } from "../../../models/project-image/project-image.model"
import { VisitorTrackingService } from "../../../services/visitor-tracking.service"

// <CHANGE> Added interface to track loading state
interface ProjectState {
  loading: boolean
  project: ProjectDetails | null
}

@Component({
  selector: "app-project-details",
  standalone: true,
  imports: [CommonModule, NavBarComponent, FooterComponent],
  templateUrl: "./project-details.component.html",
  styleUrls: ["./project-details.component.scss"],
})
export class ProjectDetailsComponent implements OnInit {
  projectState$!: Observable<ProjectState>
  
  readonly defaultProjectImage = 'https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg'

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private projectImageService: ProjectImageService,
    private mediaService: MediaService,
    private projectTagService: ProjectTagService,
    private projectTechService: ProjectTechService,
    private tagService: TagService,
    private techService: TechService,
    private visitorTrackingService: VisitorTrackingService
  ) {}

  ngOnInit(): void {
    // <CHANGE> Updated to emit loading state first, then project data
    this.projectState$ = this.route.paramMap.pipe(
      switchMap(params => {
        const projectId = params.get('id')
        if (!projectId) {
          return of({ loading: false, project: null })
        }

        this.visitorTrackingService.trackPageView(`/projects/${projectId}`, projectId).subscribe({
          next: () => console.log('Project view tracked'),
          error: (err) => console.error('Error tracking project view:', err)
        })
        
        return this.loadProjectDetails(projectId).pipe(
          map(project => ({ loading: false, project })),
          startWith({ loading: true, project: null })
        )
      })
    )
  }

  private loadProjectDetails(projectId: string): Observable<ProjectDetails | null> {
  return this.projectService.getById(projectId).pipe(
    switchMap((project) => {
      console.log('Project loaded:', project);
      console.log('Tag IDs:', project.tagIds);
      console.log('Tech IDs:', project.techIds);
      
      return forkJoin({
        project: of(project),
        images: this.loadProjectImages(projectId),
        tags: this.loadProjectTags(project.tagIds || []),
        techs: this.loadProjectTechs(project.techIds || [])
      })
    }),
    map(({ project, images, tags, techs }) => {
      console.log('Final project details:', { project, images, tags, techs });
      return {
        ...project,
        images,
        tags,
        techs
      } as ProjectDetails
    }),
    catchError((error) => {
      console.error('Error loading project details:', error)
      return of(null)
    })
  )
}

  private loadProjectImages(projectId: string): Observable<ProjectImage[]> {
  return this.projectImageService.get({ projectId, pageSize: 100 }).pipe(
    switchMap((result) => {
      const projectImages = result.items || []
      
      if (projectImages.length === 0) {
        return of([])
      }

      const imageObservables = projectImages.map(projectImage =>
        this.mediaService.getById(projectImage.mediaId).pipe(
          map(media => ({
            url: media.fileUrl,
            alt: media.altText || projectImage.caption || 'Project image',
            caption: projectImage.caption,
            order: projectImage.order
          } as ProjectImage)),
          catchError(() => of(null))
        )
      )

      return forkJoin(imageObservables).pipe(
        map(images => images.filter((img): img is ProjectImage => img !== null)
          .sort((a, b) => a.order - b.order))
      )
    }),
    catchError(() => of([]))
  )
}

  private loadProjectTags(tagIds: string[]): Observable<TagResponse[]> {
  if (!tagIds || tagIds.length === 0) {
    return of([])
  }

  const tagObservables = tagIds.map(tagId =>
    this.tagService.getById(tagId).pipe(
      catchError(() => of(null))
    )
  )

  return forkJoin(tagObservables).pipe(
    map(tags => tags.filter((tag): tag is TagResponse => tag !== null)),
    catchError(() => of([]))
  )
}

private loadProjectTechs(techIds: string[]): Observable<TechResponse[]> {
  if (!techIds || techIds.length === 0) {
    return of([])
  }

  const techObservables = techIds.map(techId =>
    this.techService.getById(techId).pipe(
      catchError(() => of(null))
    )
  )

  return forkJoin(techObservables).pipe(
    map(techs => techs.filter((tech): tech is TechResponse => tech !== null)),
    catchError(() => of([]))
  )
}

  goBack(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.router.navigate(['/projects']);
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement
    img.src = this.defaultProjectImage
  }
}