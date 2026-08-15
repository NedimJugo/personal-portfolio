using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Enums;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.Interfaces;
using Portfolio.WebAPI.BaseContoller;
using System.Xml.Linq;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController
        : BaseCRUDController<BlogPostResponse, BlogPostSearchObject, BlogPostInsertRequest, BlogPostUpdateRequest, Guid>
    {
        private const string SiteBaseUrl = "https://nedim-jugo.vercel.app";

        public BlogPostsController(IBlogPostService service, ILogger<BlogPostsController> logger)
            : base(service, logger)
        {
        }

        /// <summary>
        /// RSS 2.0 feed of the most recently published blog posts.
        /// </summary>
        [HttpGet("rss")]
        [AllowAnonymous]
        [Produces("application/rss+xml")]
        public async Task<IActionResult> GetRssFeed(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAsync(new BlogPostSearchObject
            {
                Status = BlogPostStatus.Published,
                Page = 0,
                PageSize = 50
            }, cancellationToken);

            var posts = (result.Items ?? new List<BlogPostResponse>())
                .Where(p => p.PublishedAt.HasValue)
                .OrderByDescending(p => p.PublishedAt)
                .Take(20);

            XNamespace atom = "http://www.w3.org/2005/Atom";
            var channel = new XElement("channel",
                new XElement("title", "Nedim Jugo - Blog"),
                new XElement("link", $"{SiteBaseUrl}/blog"),
                new XElement("description", "Thoughts, tutorials & musings from Nedim Jugo"),
                new XElement("language", "en-us"),
                new XElement(atom + "link",
                    new XAttribute("href", $"{Request.Scheme}://{Request.Host}/api/blogposts/rss"),
                    new XAttribute("rel", "self"),
                    new XAttribute("type", "application/rss+xml"))
            );

            foreach (var post in posts)
            {
                channel.Add(new XElement("item",
                    new XElement("title", post.Title),
                    new XElement("link", $"{SiteBaseUrl}/blog#{post.Slug}"),
                    new XElement("guid", new XAttribute("isPermaLink", "false"), post.Id.ToString()),
                    new XElement("pubDate", post.PublishedAt!.Value.ToString("R")),
                    new XElement("description", post.Excerpt)
                ));
            }

            var rss = new XElement("rss", new XAttribute("version", "2.0"), new XAttribute(XNamespace.Xmlns + "atom", atom), channel);
            var document = new XDocument(new XDeclaration("1.0", "utf-8", null), rss);

            Response.Headers.CacheControl = "public, max-age=1800";
            return Content(document.Declaration + "\n" + document.ToString(), "application/rss+xml; charset=utf-8");
        }
    }
}
