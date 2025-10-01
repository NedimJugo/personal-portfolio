import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"

@Component({
  selector: "app-nav-bar",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./nav-bar.component.html",
  styleUrls: ["./nav-bar.component.scss"],
})
export class NavBarComponent {
  isMenuOpen = false

  navLinks = [
    { label: "About", href: "#about" },
    { label: "Experience", href: "#experience" },
    { label: "Work", href: "#projects" },
    { label: "Writing", href: "#blog" },
    { label: "Contact", href: "#contact" },
  ]

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
    // Prevent body scroll when mobile menu is open
    if (this.isMenuOpen) {
      document.body.style.overflow = "hidden"
    } else {
      document.body.style.overflow = ""
    }
  }

  downloadResume(): void {
    // Implement resume download logic
    console.log("Downloading resume...")
  }
}
