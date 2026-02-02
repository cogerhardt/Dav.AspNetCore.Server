namespace Dav.AspNetCore.Server.Store.Files;

public abstract class FileStore : IStore
{
    /// <summary>
    /// Gets the item cache.
    /// </summary>
    internal Dictionary<WebDavPath, IStoreItem?> ItemCache { get; } = new();

    /// <summary>
    /// Gets the collection cache.
    /// </summary>
    internal Dictionary<WebDavPath, List<IStoreItem>> CollectionCache { get; } = new();
    
    /// <summary>
    /// A value indicating whether caching will be disabled.
    /// </summary>
    public bool DisableCaching { get; set; }

    /// <summary>
    /// Gets the store item async.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The store item or null.</returns>
    public async Task<IStoreItem?> GetItemAsync(WebDavPath uri, CancellationToken cancellationToken = default)
    {
        if (ItemCache.TryGetValue(uri, out var cacheItem) && !DisableCaching)
            return cacheItem;
        
        if (await DirectoryExistsAsync(uri, cancellationToken))
        {
            var directoryProperties = await GetDirectoryPropertiesAsync(uri, cancellationToken);
            var directory = new Directory(this, directoryProperties);

            ItemCache[directory.Uri] = directory;
            return directory;
        }

        if (await FileExistsAsync(uri, cancellationToken))
        {
            var fileProperties = await GetFilePropertiesAsync(uri, cancellationToken);
            var file = new File(this, fileProperties);

            ItemCache[file.Uri] = file;
            return file;
        }

        return null;
    }

    public abstract ValueTask<bool> DirectoryExistsAsync(WebDavPath uri, CancellationToken cancellationToken = default);

    public abstract ValueTask<bool> FileExistsAsync(WebDavPath uri, CancellationToken cancellationToken = default);

    public abstract ValueTask DeleteDirectoryAsync(WebDavPath uri, CancellationToken cancellationToken = default);
    
    public abstract ValueTask DeleteFileAsync(WebDavPath uri, CancellationToken cancellationToken = default);

    public abstract ValueTask<DirectoryProperties> GetDirectoryPropertiesAsync(WebDavPath uri, CancellationToken cancellationToken = default);

    public abstract ValueTask<FileProperties> GetFilePropertiesAsync(WebDavPath uri, CancellationToken cancellationToken = default);

    public abstract ValueTask<Stream> OpenFileStreamAsync(WebDavPath uri, OpenFileMode mode, CancellationToken cancellationToken = default);

    public abstract ValueTask CreateDirectoryAsync(WebDavPath uri, CancellationToken cancellationToken);

    public abstract ValueTask<WebDavPath[]> GetFilesAsync(WebDavPath uri, CancellationToken cancellationToken);
    
    public abstract ValueTask<WebDavPath[]> GetDirectoriesAsync(WebDavPath uri, CancellationToken cancellationToken);
}