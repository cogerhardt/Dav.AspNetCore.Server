namespace Dav.AspNetCore.Server.Store.Files;

public class LocalFileStore : FileStore
{
    private readonly LocalFileStoreOptions options;

    /// <summary>
    /// Initializes a new <see cref="LocalFileStore"/> class.
    /// </summary>
    /// <param name="options">The local file store options.</param>
    public LocalFileStore(LocalFileStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        this.options = options;
    }

    public override ValueTask<bool> DirectoryExistsAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        return ValueTask.FromResult(System.IO.Directory.Exists(path));
    }

    public override ValueTask<bool> FileExistsAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        return ValueTask.FromResult(System.IO.File.Exists(path));
    }

    public override ValueTask DeleteDirectoryAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        System.IO.Directory.Delete(path);
        
        return ValueTask.CompletedTask;
    }

    public override ValueTask DeleteFileAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        System.IO.File.Delete(path);
        
        return ValueTask.CompletedTask;
    }

    public override ValueTask<DirectoryProperties> GetDirectoryPropertiesAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        var directoryInfo = new DirectoryInfo(path);
        var directoryProperties = new DirectoryProperties(
            uri,
            directoryInfo.Name,
            directoryInfo.CreationTimeUtc,
            directoryInfo.LastWriteTimeUtc);

        return ValueTask.FromResult(directoryProperties);
    }

    public override ValueTask<FileProperties> GetFilePropertiesAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        var fileInfo = new FileInfo(path);
        var fileProperties = new FileProperties(
            uri,
            fileInfo.Name,
            fileInfo.CreationTimeUtc,
            fileInfo.LastWriteTimeUtc,
            fileInfo.Length);

        return ValueTask.FromResult(fileProperties);
    }

    public override ValueTask<Stream> OpenFileStreamAsync(WebDavPath uri, OpenFileMode mode, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        return ValueTask.FromResult<Stream>(mode == OpenFileMode.Read 
            ? System.IO.File.OpenRead(path) 
            : System.IO.File.OpenWrite(path));
    }

    public override ValueTask CreateDirectoryAsync(WebDavPath uri, CancellationToken cancellationToken)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        System.IO.Directory.CreateDirectory(path);
        
        return ValueTask.CompletedTask;
    }

    public override ValueTask<WebDavPath[]> GetFilesAsync(WebDavPath uri, CancellationToken cancellationToken)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        return ValueTask.FromResult(System.IO.Directory.GetFiles(path).Select(x =>
        {
            var relativePath = $"/{Path.GetRelativePath(options.RootPath, x)}";
            return WebDavPath.FromString(relativePath);
        }).ToArray());
    }

    public override ValueTask<WebDavPath[]> GetDirectoriesAsync(WebDavPath uri, CancellationToken cancellationToken)
    {
        var path = Path.Combine(options.RootPath, uri.LocalPath.TrimStart('/'));
        return ValueTask.FromResult(System.IO.Directory.GetDirectories(path).Select(x =>
        {
            var relativePath = $"/{Path.GetRelativePath(options.RootPath, x)}";
            return WebDavPath.FromString(relativePath);
        }).ToArray());
    }
}