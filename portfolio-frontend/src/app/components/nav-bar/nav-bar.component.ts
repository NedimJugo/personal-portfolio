import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router" // Add this import

@Component({
  selector: "app-nav-bar",
  standalone: true,
  imports: [CommonModule, RouterModule], // Add RouterModule here
  templateUrl: "./nav-bar.component.html",
  styleUrls: ["./nav-bar.component.scss"],
})
export class NavBarComponent {
  isMenuOpen = false

  // Update href to use router links
  navLinks = [
    { label: "About", href: "/about" },
    { label: "Experience", href: "/experience" },
    { label: "Work", href: "/projects" }, // Changed from #projects to /projects
    { label: "Writing", href: "/blog" },
    { label: "Contact", href: "/contact" },
  ]

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
    if (this.isMenuOpen) {
      document.body.style.overflow = "hidden"
    } else {
      document.body.style.overflow = ""
    }
  }

  downloadResume(): void {
    console.log("Downloading resume...")
  }
}