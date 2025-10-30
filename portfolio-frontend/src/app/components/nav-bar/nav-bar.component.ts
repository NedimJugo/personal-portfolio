import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"
import { SiteContentService } from "../../services/site-content.service" // Add this import
import { map, catchError } from "rxjs/operators" // Add this import
import { of } from "rxjs" // Add this import

@Component({
  selector: "app-nav-bar",
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: "./nav-bar.component.html",
  styleUrls: ["./nav-bar.component.scss"],
})
export class NavBarComponent {
  isMenuOpen = false

  navLinks = [
    { label: "About", href: "/about" },
    { label: "Experience", href: "/experience" },
    { label: "Work", href: "/projects" },
    { label: "Writing", href: "/blog" },
    { label: "Contact", href: "/contact" },
  ]

  constructor(private siteContentService: SiteContentService) {} // Add constructor

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
    if (this.isMenuOpen) {
      document.body.style.overflow = "hidden"
    } else {
      document.body.style.overflow = ""
    }
  }

  downloadResume(): void {
    this.siteContentService.get({ section: 'resume', isPublished: true }).pipe(
      map((result) => {
        if (result.items && result.items.length > 0) {
          const resumeData = result.items[0];
          if (resumeData.contentType === 'json') {
            const resumeInfo = JSON.parse(resumeData.content);
            // Open the resume URL in a new tab
            window.open(resumeInfo.fileUrl, '_blank');
          }
        } else {
          console.error('No resume found');
          alert('Resume not available at the moment');
        }
      }),
      catchError((error) => {
        console.error('Error fetching resume:', error);
        alert('Failed to load resume');
        return of(null);
      })
    ).subscribe();
  }
}