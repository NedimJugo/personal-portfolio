import { Component, type OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from "@angular/core"
import { CommonModule } from "@angular/common"
import { type Observable, of } from "rxjs"
import { map, catchError } from "rxjs/operators"
import { ParsedExperience } from "../../models/experience/parsed-experience.model"
import { SkillResponse } from "../../models/skill/skill-response.model"
import { TestimonialResponse } from "../../models/testimonial/testimonial-response.model"
import { ExperienceService } from "../../services/experience.service"
import { SkillService } from "../../services/skill.service"
import { TestimonialService } from "../../services/testimonal.service"
import { ExperienceResponse } from "../../models/experience/experience-response.model"
import { FooterComponent } from "../footer/footer.component";
import { NavBarComponent } from "../nav-bar/nav-bar.component";

@Component({
  selector: "app-experiences",
  standalone: true,
  imports: [CommonModule, FooterComponent, NavBarComponent],
  templateUrl: "./experiences.component.html",
  styleUrls: ["./experiences.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExperiencesComponent implements OnInit, OnDestroy {
  experiences$!: Observable<ParsedExperience[]>
  skills$!: Observable<SkillResponse[]>
  testimonials$!: Observable<TestimonialResponse[]>
  currentSlide = 0;
  private testimonials: TestimonialResponse[] = [];
  private autoAdvanceInterval: any;

  constructor(
    private experienceService: ExperienceService,
    private skillService: SkillService,
    private testimonialService: TestimonialService,
    private cdr: ChangeDetectorRef  // Add ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.experiences$ = this.experienceService.get({ retrieveAll: true }).pipe(
      map((result) => {
        const items = result.items || []
        return items.map((exp) => this.parseExperience(exp))
      }),
      catchError(() => of([])),
    )

    this.skills$ = this.skillService.get({ retrieveAll: true }).pipe(
      map((result) => result.items || []),
      catchError(() => of([])),
    )

    this.testimonials$ = this.testimonialService
      .get({
        isApproved: true,
        retrieveAll: true,
      })
      .pipe(
        map((result) => {
          const items = result.items || [];
          this.testimonials = items;
          // Start auto-advance after testimonials are loaded
          this.startAutoAdvance();
          return items;
        }),
        catchError(() => of([])),
      );
  }

  private startAutoAdvance(): void {
    // Clear any existing interval
    if (this.autoAdvanceInterval) {
      clearInterval(this.autoAdvanceInterval);
    }

    // Only start if we have testimonials
    if (this.testimonials.length > 1) {
      this.autoAdvanceInterval = setInterval(() => {
        this.nextSlide();
      }, 5000);
    }
  }

  nextSlide(): void {
    if (this.testimonials.length === 0) return;
    
    this.currentSlide = (this.currentSlide + 1) % this.testimonials.length;
    this.cdr.markForCheck(); // Trigger change detection
  }

  previousSlide(): void {
    if (this.testimonials.length === 0) return;
    
    this.currentSlide = this.currentSlide === 0 
      ? this.testimonials.length - 1 
      : this.currentSlide - 1;
    this.cdr.markForCheck(); // Trigger change detection
  }

  goToSlide(index: number): void {
    if (index >= 0 && index < this.testimonials.length) {
      this.currentSlide = index;
      this.cdr.markForCheck(); // Trigger change detection
    }
  }

  private parseExperience(exp: ExperienceResponse): ParsedExperience {
    let achievementsList: string[] = []
    let technologiesList: string[] = []

    try {
      achievementsList = exp.achievements ? JSON.parse(exp.achievements) : []
    } catch {
      achievementsList = []
    }

    try {
      technologiesList = exp.technologies ? JSON.parse(exp.technologies) : []
    } catch {
      technologiesList = []
    }

    return {
      ...exp,
      achievementsList,
      technologiesList,
    }
  }

  getStars(rating: number): number[] {
    return Array(Math.floor(rating)).fill(0)
  }

  getProficiencyLabel(level: number): string {
    if (level >= 90) return "Expert"
    if (level >= 75) return "Advanced"
    if (level >= 50) return "Intermediate"
    return "Beginner"
  }

  getSkillCategories(skills: SkillResponse[] | null): string[] {
    if (!skills) return []
    const categories = new Set(skills.map((skill) => skill.category))
    return Array.from(categories).sort()
  }

  getSkillsByCategory(skills: SkillResponse[] | null, category: string): SkillResponse[] {
    if (!skills) return []
    return skills.filter((skill) => skill.category === category).sort((a, b) => a.displayOrder - b.displayOrder)
  }

  ngOnDestroy(): void {
    if (this.autoAdvanceInterval) {
      clearInterval(this.autoAdvanceInterval);
    }
  }
}