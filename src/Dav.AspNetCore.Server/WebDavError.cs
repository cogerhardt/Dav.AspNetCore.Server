using System.Xml.Linq;

namespace Dav.AspNetCore.Server;

internal record WebDavError(
    WebDavPath Uri,
    DavStatusCode StatusCode,
    XElement? ErrorElement);