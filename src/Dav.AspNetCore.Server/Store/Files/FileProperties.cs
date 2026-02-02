namespace Dav.AspNetCore.Server.Store.Files;

public record FileProperties(
    WebDavPath Uri,
    string Name,
    DateTime Created,
    DateTime LastModified,
    long Length);