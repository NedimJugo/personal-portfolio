import { Component, type OnInit, ChangeDetectionStrategy } from "@angular/core"
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
export class ExperiencesComponent implements OnInit {
  experiences$!: Observable<ParsedExperience[]>
  skills$!: Observable<SkillResponse[]>
  testimonials$!: Observable<TestimonialResponse[]>

  constructor(
    private experienceService: ExperienceService,
    private skillService: SkillService,
    private testimonialService: TestimonialService,
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
        map((result) => result.items || []),
        catchError(() => of([])),
      )
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
}
