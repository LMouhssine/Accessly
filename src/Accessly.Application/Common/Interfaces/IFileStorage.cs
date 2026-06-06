namespace Accessly.Application.Common.Interfaces;

/// <summary>Stores binary content (for example event cover images) and returns a public URL.</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}
