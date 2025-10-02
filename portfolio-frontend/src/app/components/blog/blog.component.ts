import { Component, type OnInit, ChangeDetectionStrategy } from "@angular/core"
import { CommonModule } from "@angular/common"
import { type Observable, of } from "rxjs"
import { map, catchError } from "rxjs/operators"
import { BlogPostResponse } from "../../models/blog-post/blog-post-response.model"
import { BlogPostService } from "../../services/blog-post.service"
import { BlogPostStatus } from "../../models/enums/blog-post-status.enum"
import { NavBarComponent } from "../nav-bar/nav-bar.component";
import { FooterComponent } from "../footer/footer.component";
import { BlogPostWithParsedData } from "../../models/blog-post/blog-post-with-parsed-data.model"


@Component({
  selector: "app-blog",
  standalone: true,
  imports: [CommonModule, NavBarComponent, FooterComponent],
  templateUrl: "./blog.component.html",
  styleUrls: ["./blog.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BlogComponent implements OnInit {
  blogPosts$!: Observable<BlogPostWithParsedData[]>
  featuredPost$!: Observable<BlogPostWithParsedData | null>
  categories$!: Observable<string[]>

  selectedPost: BlogPostWithParsedData | null = null
  selectedCategory: string | null = null

  readonly defaultBlogImage =
    "https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg"

  constructor(private blogService: BlogPostService) {}

  ngOnInit(): void {
    this.loadBlogPosts()
    this.loadFeaturedPost()
    this.loadCategories()
  }

  private loadBlogPosts(): void {
    this.blogPosts$ = this.blogService
      .get({
        status: BlogPostStatus.Published,
        pageSize: 50,
      })
      .pipe(
        map((result) => this.parseBlogPosts(result.items || [])),
        catchError(() => of([])),
      )
  }

  private loadFeaturedPost(): void {
    this.featuredPost$ = this.blogService
      .get({
        status: BlogPostStatus.Published,
        pageSize: 1,
        sortBy: "viewCount",
        desc: true,
      })
      .pipe(
        map((result) => {
          const posts = this.parseBlogPosts(result.items || [])
          return posts.length > 0 ? posts[0] : null
        }),
        catchError(() => of(null)),
      )
  }

  private loadCategories(): void {
    this.categories$ = this.blogPosts$.pipe(
      map((posts) => {
        const allCategories = posts.flatMap((post) => post.parsedCategories)
        return [...new Set(allCategories)].filter((cat) => cat.length > 0)
      }),
    )
  }

  private parseBlogPosts(posts: BlogPostResponse[]): BlogPostWithParsedData[] {
    return posts.map((post) => ({
      ...post,
      parsedTags: this.parseJsonArray(post.tags),
      parsedCategories: this.parseJsonArray(post.categories),
    }))
  }

  private parseJsonArray(jsonString: string): string[] {
    try {
      const parsed = JSON.parse(jsonString)
      return Array.isArray(parsed) ? parsed : []
    } catch {
      return []
    }
  }

  selectPost(post: BlogPostWithParsedData): void {
    this.selectedPost = post
    window.scrollTo({ top: 0, behavior: "smooth" })
  }

  backToList(): void {
    this.selectedPost = null
    window.scrollTo({ top: 0, behavior: "smooth" })
  }

  filterByCategory(category: string): void {
    this.selectedCategory = this.selectedCategory === category ? null : category

    if (this.selectedCategory) {
      this.blogPosts$ = this.blogService
        .get({
          status: BlogPostStatus.Published,
          pageSize: 50,
        })
        .pipe(
          map((result) => {
            const parsed = this.parseBlogPosts(result.items || [])
            return parsed.filter((post) => post.parsedCategories.includes(this.selectedCategory!))
          }),
          catchError(() => of([])),
        )
    } else {
      this.loadBlogPosts()
    }
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement
    img.src = this.defaultBlogImage
  }

  getImageUrl(post: BlogPostWithParsedData): string {
    return post.featuredImage || `${this.defaultBlogImage}?query=${encodeURIComponent(post.title)}`
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString)
    return date.toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    })
  }
}