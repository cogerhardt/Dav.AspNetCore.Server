using Microsoft.AspNetCore.Http;

namespace Dav.AspNetCore.Server;

internal static class PathStringExtensions
{
    public static Uri ToUri(this PathString path, String? queryString = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new Uri("/" + queryString, UriKind.Relative);

        String uri = Uri.UnescapeDataString(path.ToUriComponent().TrimEnd('/')) + queryString;

        if (string.IsNullOrWhiteSpace(uri))
            return new Uri("/" + queryString, UriKind.Relative);

        // should work for linux and windows
        if (Uri.TryCreate(uri, UriKind.Relative, out var relUri))
        {
            return relUri;
        }

        // Fallback
        if (Uri.TryCreate(uri, UriKind.Absolute, out var absUri))
        {
            return absUri;
        }

        throw new UriFormatException($"Invalid path '{uri}'");
    }
}