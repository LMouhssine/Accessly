using Accessly.Application.Common.Interfaces;
using QRCoder;

namespace Accessly.Infrastructure.QrCodes;

/// <summary>Generates PNG QR codes with QRCoder (no System.Drawing dependency).</summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    public byte[] GeneratePng(string payload, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(data);
        return pngQr.GetGraphic(pixelsPerModule);
    }
}
