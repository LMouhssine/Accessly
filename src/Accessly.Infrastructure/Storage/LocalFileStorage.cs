using Accessly.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Accessly.Infrastructure.Storage;

/// <summary>Stores uploaded files on the local filesystem and serves them as static content.</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly string _publicBasePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _root = configuration["Storage:LocalPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _publicBasePath = (configuration["Storage:PublicBasePath"] ?? "/uploads").TrimEnd('/');
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_root, storedName);

        await using (var target = File.Create(fullPath))
        {
            await content.CopyToAsync(target, cancellationToken);
        }

        return $"{_publicBasePath}/{storedName}";
    }
}
