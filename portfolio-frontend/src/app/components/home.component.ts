import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { forkJoin, type Observable, of } from "rxjs"
import { catchError, map, switchMap } from "rxjs/operators"
import { ProjectResponse } from "../models/project/project-response.model"
import { NavBarComponent } from "./nav-bar/nav-bar.component"
import { BlogPostResponse } from "../models/blog-post/blog-post-response.model"
import { ExperienceResponse } from "../models/experience/experience-response.model"
import { SkillResponse } from "../models/skill/skill-response.model"
import { TestimonialResponse } from "../models/testimonial/testimonial-response.model"
import { SocialLinkResponse } from "../models/social-link/social-link-response.model"
import { ProjectService } from "../services/project.service"
import { BlogPostService } from "../services/blog-post.service"
import { ExperienceService } from "../services/experience.service"
import { SkillService } from "../services/skill.service"
import { TestimonialService } from "../services/testimonal.service"
import { SocialLinkService } from "../services/social-link.service"
import { DEFAULT_HERO_CONTENT, HeroContent } from "../models/hero/hero-content.model"
import { SiteContentService } from "../services/site-content.service"
import { SiteContentResponse } from "../models/site-content/site-content-response.model"
import { FooterComponent } from "./footer/footer.component";
import { ProjectImageService } from "../services/project-image.service"
import { MediaService } from "../services/media.service"
import { ProjectWithImage } from "../models/project-image/project-with-image.model"


@Component({
  selector: "app-home",
  standalone: true,
  imports: [CommonModule, NavBarComponent, FooterComponent],
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.scss"],
})
export class HomeComponent implements OnInit {
  featuredProjects$!: Observable<ProjectWithImage[]>
  latestBlogPosts$!: Observable<BlogPostResponse[]>
  experiences$!: Observable<ExperienceResponse[]>
  skills$!: Observable<SkillResponse[]>
  testimonials$!: Observable<TestimonialResponse[]>
  heroContent$!: Observable<HeroContent>

  readonly defaultProjectImage = 'https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg'

  newsletterEmail = ""
  newsletterSubscribed = false

  isFlipped = false;
  flipPhotoUrl = ''; // You can set a default or load from DB
  flipPhotoCaption = 'This is me in action!';

  constructor(
    private projectService: ProjectService,
    private blogService: BlogPostService,
    private experienceService: ExperienceService,
    private skillService: SkillService,
    private testimonialService: TestimonialService,
    private siteContentService: SiteContentService,
    private projectImageService: ProjectImageService,
    private mediaService: MediaService
  ) {}

  ngOnInit(): void {

    this.heroContent$ = this.loadHeroContent();

    // Load featured projects with images
    this.featuredProjects$ = this.loadProjectsWithImages();

    // Load latest blog posts
    this.latestBlogPosts$ = this.blogService.get({ pageSize: 3 }).pipe(
      map((result) => result.items || []),
      catchError(() => of([])),
    )

    // Load experiences
    this.experiences$ = this.experienceService.get({ pageSize: 3 }).pipe(
      map((result) => result.items || []),
      catchError(() => of([])),
    )

    // Load featured skills
    this.skills$ = this.skillService.get({ isFeatured: true, pageSize: 6 }).pipe(
      map((result) => result.items || []),
      catchError(() => of([])),
    )

    // Load testimonials with safe fallback
    this.testimonials$ = this.testimonialService.get({ isApproved: true, pageSize: 3 }).pipe(
      map((result) => result.items || []),
      catchError(() => of([])),
    )
  }

  private loadProjectsWithImages(): Observable<ProjectWithImage[]> {
    return this.projectService.get({ isFeatured: true, pageSize: 3 }).pipe(
      switchMap((result) => {
        const projects = result.items || [];
        
        if (projects.length === 0) {
          return of([]);
        }

        // For each project, load its hero image
        const projectsWithImages$ = projects.map(project => 
          this.loadProjectHeroImage(project)
        );

        return forkJoin(projectsWithImages$);
      }),
      catchError((error) => {
        console.error('Error loading projects with images:', error);
        return of([]);
      })
    );
  }

  private loadProjectHeroImage(project: ProjectResponse): Observable<ProjectWithImage> {
    return this.projectImageService.get({ 
      projectId: project.id, 
      isHero: true,
      pageSize: 1 
    }).pipe(
      switchMap((imageResult) => {
        const projectImage = imageResult.items?.[0];
        
        if (!projectImage) {
          // No hero image found, return project with placeholder
          return of({
            ...project,
            heroImageUrl: `https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg?height=300&width=400&query=${encodeURIComponent(project.title)}`,
            heroImageAlt: project.title
          } as ProjectWithImage);
        }

        // Load the actual media file
        return this.mediaService.getById(projectImage.mediaId).pipe(
          map((media) => ({
            ...project,
            heroImageUrl: media.fileUrl,
            heroImageAlt: media.altText || projectImage.caption || project.title
          } as ProjectWithImage)),
          catchError(() => {
            // If media loading fails, use placeholder
            return of({
              ...project,
              heroImageUrl: `https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg?height=300&width=400&query=${encodeURIComponent(project.title)}`,
              heroImageAlt: project.title
            } as ProjectWithImage);
          })
        );
      }),
      catchError(() => {
        // If image loading fails, return project with placeholder
        return of({
          ...project,
          heroImageUrl: `/placeholder.svg?height=300&width=400&query=${encodeURIComponent(project.title)}`,
          heroImageAlt: project.title
        } as ProjectWithImage);
      })
    );
  }

  onFlipCardHover(): void {
    this.isFlipped = true;
  }

  onFlipCardLeave(): void {
    this.isFlipped = false;
  }

  toggleFlip(): void {
    this.isFlipped = !this.isFlipped;
  }

   private loadHeroContent(): Observable<HeroContent> {
  return this.siteContentService.get({ section: 'hero', isPublished: true }).pipe(
    map((result) => {
      if (result.items && result.items.length > 0) {
        const heroData = result.items.find(item => item.contentType === 'json');
        
        if (heroData) {
          return this.parseHeroContent(heroData);
        }
      }
      return DEFAULT_HERO_CONTENT;
    }),
    catchError(() => of(DEFAULT_HERO_CONTENT))
  );
}

  private parseHeroContent(siteContent: SiteContentResponse): HeroContent {
    try {
      if (siteContent.contentType === 'json') {
        return JSON.parse(siteContent.content);
      } else {
        console.warn('Hero content is not in JSON format, using default');
        return DEFAULT_HERO_CONTENT;
      }
    } catch (error) {
      console.error('Error parsing hero content:', error);
      return DEFAULT_HERO_CONTENT;
    }
  }

  subscribeNewsletter(): void {
    if (this.newsletterEmail) {
      this.newsletterSubscribed = true
      console.log("Newsletter subscription:", this.newsletterEmail)

      setTimeout(() => {
        this.newsletterSubscribed = false
        this.newsletterEmail = ""
      }, 3000)
    }
  }

  downloadResume(): void {
    console.log("Downloading resume...")
  }

  viewProjects(): void {
    window.location.href = "#projects"
  }

  contactMe(): void {
    window.location.href = "#contact"
  }

  getStars(rating: number): number[] {
    return Array(Math.floor(rating)).fill(0)
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.defaultProjectImage;
  }
}
