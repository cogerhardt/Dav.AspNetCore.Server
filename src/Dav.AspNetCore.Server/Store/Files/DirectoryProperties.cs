namespace Dav.AspNetCore.Server.Store.Files;

public record DirectoryProperties(
    WebDavPath Uri,
    string Name,
    DateTime Created,
    DateTime LastModified);