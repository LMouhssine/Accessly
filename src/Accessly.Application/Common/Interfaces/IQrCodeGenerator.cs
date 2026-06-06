namespace Accessly.Application.Common.Interfaces;

/// <summary>Renders QR codes for ticket payloads.</summary>
public interface IQrCodeGenerator
{
    byte[] GeneratePng(string payload, int pixelsPerModule = 10);
}
