import { Component, type OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from "@angular/core"
import { CommonModule } from "@angular/common"
import { forkJoin, type Observable, of } from "rxjs"
import { map, catchError, switchMap } from "rxjs/operators"
import { BlogPostResponse } from "../../models/blog-post/blog-post-response.model"
import { BlogPostService } from "../../services/blog-post.service"
import { BlogPostStatus } from "../../models/enums/blog-post-status.enum"
import { NavBarComponent } from "../nav-bar/nav-bar.component";
import { FooterComponent } from "../footer/footer.component";
import { BlogPostWithParsedData } from "../../models/blog-post/blog-post-with-parsed-data.model"
import { VisitorTrackingService } from "../../services/visitor-tracking.service"
import { BlogPostLikeService } from "../../services/blog-post-like.service"
import { MediaService } from "../../services/media.service"



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
  visitorId: string = ''

  readonly defaultBlogImage =
    "https://ecochallengeblob.blob.core.windows.net/ecochallenge/istockphoto-2173059563-612x612.jpg"

  constructor(private blogService: BlogPostService,
    private visitorTrackingService: VisitorTrackingService,
    private blogPostLikeService: BlogPostLikeService,
    private mediaService: MediaService, 
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.visitorId = this.visitorTrackingService.getOrCreateVisitorId()
    this.visitorTrackingService.trackPageView('/blog').subscribe({
      next: () => console.log('Blog page view tracked'),
      error: (err) => console.error('Error tracking page view:', err)
    })
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
      switchMap((posts) => this.resolveMediaUrls(posts)),
      switchMap((posts) => this.loadLikeStatusForPosts(posts)),
      catchError(() => of([])),
    )
}

  private resolveMediaUrls(posts: BlogPostWithParsedData[]): Observable<BlogPostWithParsedData[]> {
  const postsWithMediaIds = posts.filter(post => 
    post.featuredImage && 
    !post.featuredImage.startsWith('http')
  )

  if (postsWithMediaIds.length === 0) {
    return of(posts)
  }

  const mediaRequests = postsWithMediaIds.map(post =>
    this.mediaService.generateSasUrl(post.featuredImage!, 24).pipe(
      map(response => ({ postId: post.id, url: response.sasUrl })),
      catchError(() => of({ postId: post.id, url: this.defaultBlogImage }))
    )
  )

  return forkJoin(mediaRequests).pipe(
    map(mediaUrls => {
      const urlMap = new Map(mediaUrls.map(m => [m.postId, m.url]))
      return posts.map(post => ({
        ...post,
        resolvedImageUrl: urlMap.get(post.id) || post.featuredImage || this.defaultBlogImage
      }))
    })
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
      switchMap((post) => {
        if (!post) return of(null)
        return this.resolveMediaUrls([post]).pipe(map(posts => posts[0]))
      }),
      switchMap((post) => {
        if (!post) return of(null)
        return this.loadLikeStatusForPost(post)
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

  private loadLikeStatusForPosts(posts: BlogPostWithParsedData[]): Observable<BlogPostWithParsedData[]> {
    if (posts.length === 0) return of([])

    const likeStatusRequests = posts.map(post =>
      this.blogPostLikeService.getLikeStatus(post.id, this.visitorId).pipe(
        map(status => ({ ...post, likeStatus: status })),
        catchError(() => of({ ...post, likeStatus: { isLiked: false, totalLikes: post.likeCount } }))
      )
    )

    return forkJoin(likeStatusRequests)
  }

  private loadLikeStatusForPost(post: BlogPostWithParsedData): Observable<BlogPostWithParsedData> {
    return this.blogPostLikeService.getLikeStatus(post.id, this.visitorId).pipe(
      map(status => ({ ...post, likeStatus: status })),
      catchError(() => of({ ...post, likeStatus: { isLiked: false, totalLikes: post.likeCount } }))
    )
  }

  toggleLike(post: BlogPostWithParsedData, event: Event): void {
    event.stopPropagation()

    this.blogPostLikeService.toggleLike(post.id, this.visitorId).subscribe({
      next: (status) => {
        post.likeStatus = status
        post.likeCount = status.totalLikes
        this.cdr.markForCheck()
      },
      error: (err) => {
        console.error('Error toggling like:', err)
      }
    })
  }
  

  selectPost(post: BlogPostWithParsedData): void {
    this.selectedPost = post
    window.scrollTo({ top: 0, behavior: "smooth" })

    // Track blog post view
    this.visitorTrackingService.trackPageView(`/blog/${post.slug}`, undefined, post.id).subscribe({
      next: () => console.log('Blog post view tracked'),
      error: (err) => console.error('Error tracking blog post view:', err)
    })
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
          switchMap((posts) => this.loadLikeStatusForPosts(posts)),
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
  // Use resolved URL if available
  if ((post as any).resolvedImageUrl) {
    return (post as any).resolvedImageUrl
  }
  
  // If it's already a full URL
  if (post.featuredImage?.startsWith('http')) {
    return post.featuredImage
  }
  
  // Fallback
  return this.defaultBlogImage
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