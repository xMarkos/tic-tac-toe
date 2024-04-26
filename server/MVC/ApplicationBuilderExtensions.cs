using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace Markos.TicTacToe.MVC;

internal static class ApplicationBuilderExtensions
{
    private readonly static FileExtensionContentTypeProvider _mimeProvider = new();

    /// <summary>
    /// Tries to serve a static file.
    /// </summary>
    /// <param name="file">The file to be served.</param>
    /// <returns>True if the file exists and is served; false otherwise.</returns>
    /// <remarks>
    /// Implementation is taken from decompiled <see cref="StaticFileMiddleware" />.
    /// It respects If-Modified-Since and If-None-Match caching headers.
    /// </remarks>
    public static async Task<bool> TryServeStaticFile(this HttpContext context, FileInfo file)
    {
        if (!file.Exists)
            return false;

        DateTimeOffset lastModified;
        EntityTagHeaderValue etag;
        string? contentType;
        string path;
        long length;
        {
            path = file.FullName;
            length = file.Length;
            lastModified = file.LastWriteTimeUtc;
            lastModified = new DateTimeOffset(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second, lastModified.Offset).ToUniversalTime();

            long etagHash = lastModified.ToFileTime() ^ length;
            etag = new('\"' + Convert.ToString(etagHash, 16) + '\"');

            _mimeProvider.TryGetContentType(path, out contentType);
        }

        bool hasPrecondition = false;
        bool isModified = false;
        {
            RequestHeaders requestHeaders = context.Request.GetTypedHeaders();

            IList<EntityTagHeaderValue> ifNoneMatch = requestHeaders.IfNoneMatch;
            if (ifNoneMatch?.Count > 0)
            {
                bool shouldProcess = true;
                hasPrecondition = true;

                foreach (EntityTagHeaderValue header in ifNoneMatch)
                {
                    if (header.Equals(EntityTagHeaderValue.Any) || header.Compare(etag, useStrongComparison: true))
                    {
                        shouldProcess = false;
                        break;
                    }
                }

                if (shouldProcess)
                    isModified = true;
            }

            if (!isModified)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                DateTimeOffset? ifModifiedSince = requestHeaders.IfModifiedSince;

                if (ifModifiedSince.HasValue && ifModifiedSince <= now)
                {
                    hasPrecondition = true;

                    if (ifModifiedSince < lastModified)
                        isModified = true;
                }
            }
        }

        if (contentType != null)
            context.Response.ContentType = contentType;

        ResponseHeaders headers = context.Response.GetTypedHeaders();
        headers.LastModified = lastModified;
        headers.ETag = etag;

        if (hasPrecondition && !isModified)
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentLength = length;

            await context.Response.SendFileAsync(path, context.RequestAborted).ConfigureAwait(false);
        }

        return true;
    }

    /// <summary>
    /// Remaps requests matched by a prefix to a fallback file. This is useful for SPAs hosted in a subfolder.
    /// </summary>
    /// <param name="pattern">The prefix pattern which should be remapped. It may contain parameters which can be resolved using fallbackValues prameter.</param>
    /// <param name="fallbackValues">Values that will be used to replace parameters in the pattern.</param>
    public static IApplicationBuilder RemapMissingFile(this IApplicationBuilder app, string pattern, RouteValueDictionary fallbackValues)
    {
        IWebHostEnvironment environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        string webRootPath = environment.WebRootPath;

        RouteTemplate template = new(RoutePatternFactory.Parse(pattern));
        TemplateMatcher matcher = new(template, []);

        TemplateBinderFactory templateBinderFactory = app.ApplicationServices.GetRequiredService<TemplateBinderFactory>();
        FileInfo file =
            new(Path.Join(
                webRootPath,
                templateBinderFactory
                    .Create(template, [])
                    .BindValues(new(fallbackValues))));

        return app.Use(async (context, next) =>
        {
            // This is an implementation of app.MapFallbackToFile("/a/{**rest}", "/a/index.html") which actually works.
            // The problem with using the above is that requests to static files like /a/foo.txt stop working and the fallback is always served.
            // I believe this is actually a bug in the implementation because the behavior does not occur if the prefix pattern is not used.

            RouteValueDictionary values = [];

            if ("get".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase)
                && context.Request.Path.Value is string path
                && matcher.TryMatch(path, values)
                && !File.Exists(Path.Join(webRootPath, path)))
            {
                if (await context.TryServeStaticFile(file).ConfigureAwait(false))
                    return;
            }

            await next(context).ConfigureAwait(false);
        });
    }
}
