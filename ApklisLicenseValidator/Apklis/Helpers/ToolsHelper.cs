using ApklisLicenseValidator.Apklis.Models;
using QRCoder;
using SkiaSharp;
using System.Reflection;

namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Clase de utilidades generales para la biblioteca de validación de licencias.
    /// </summary>
    public static class ToolsHelper
    {
        /// <summary>
        /// Devuelve un texto amigable que describe cuándo expira la licencia.
        /// </summary>
        /// <param name="expirationDate">Fecha de expiración de la licencia.</param>
        /// <returns>Cadena en español describiendo el tiempo restante (ej. "en 5 días").</returns>
        public static string GetLicenseExpirationText(DateTime expirationDate)
        {
            int remainingDays = (expirationDate.Date - DateTime.Now.Date).Days;

            return remainingDays switch
            {
                <= 0 => "ya expiró.",
                1 => "mañana.",
                2 => "en 2 días.",
                _ => $"en {remainingDays} días."
            };
        }

        /// <summary>
        /// Genera un <see cref="ImageSource"/> con el código QR para el pago por Transfermóvil.
        /// Si existe el recurso del logotipo de Apklis embebido, lo superpone en el centro del QR.
        /// </summary>
        /// <param name="qrCode">Datos del código QR a codificar.</param>
        /// <returns>Un <see cref="ImageSource"/> con la imagen del QR generado.</returns>
        public static ImageSource GenerateQr(QrCode qrCode)
        {
            var assembly = typeof(SignatureVerificationService).GetTypeInfo().Assembly;
            const string logoResourceName = "ApklisLicenseValidator.Resources.Raw.apklist.png";

            using var logoStream = assembly.GetManifestResourceStream(logoResourceName);
            if (logoStream != null)
            {
                using var memoryStream = new MemoryStream();
                logoStream.CopyTo(memoryStream);
                return BuildQrImage(qrCode, memoryStream.ToArray());
            }

            return BuildQrImage(qrCode);
        }

        /// <summary>
        /// Construye la imagen del código QR usando SkiaSharp y QRCoder.
        /// Si se proveen bytes del logotipo, se superpone en el centro respetando la transparencia.
        /// </summary>
        /// <param name="qrCode">Datos del código QR.</param>
        /// <param name="logoBytes">Bytes de la imagen del logotipo (opcional).</param>
        /// <returns>Un <see cref="ImageSource"/> listo para usar en controles de MAUI.</returns>
        private static ImageSource BuildQrImage(QrCode qrCode, byte[]? logoBytes = null)
        {
            try
            {
                string qrJsonPayload = qrCode.ToJsonString();

                var qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrJsonPayload, QRCodeGenerator.ECCLevel.Q);

                // Genera el PNG como arreglo de bytes
                var pngQrCode = new PngByteQRCode(qrCodeData);
                byte[] qrImageBytes = pngQrCode.GetGraphic(20);

                // Si se provee un logo, se superpone en el centro del QR
                if (logoBytes != null)
                {
                    try
                    {
                        using var qrBitmapDecoded = SKBitmap.Decode(qrImageBytes);
                        // Convierte a RGBA para preservar los colores del logo
                        using var qrBitmap = qrBitmapDecoded.Copy(SKColorType.Rgba8888);
                        using var logoBitmap = SKBitmap.Decode(logoBytes);

                        if (logoBitmap == null)
                            throw new InvalidOperationException("El logotipo provisto no es válido.");

                        // El logo ocupa el 20% del QR (ECC nivel Q tolera hasta 25% de daño).
                        int logoSize = qrBitmap.Width / 5;
                        int logoPadding = logoSize / 8;
                        int logoInnerSize = logoSize - logoPadding * 2;

                        using var scaledLogo = logoBitmap.Resize(
                            new SKImageInfo(logoInnerSize, logoInnerSize, SKColorType.Rgba8888, SKAlphaType.Premul),
                            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                        using var canvas = new SKCanvas(qrBitmap);

                        int centerX = (qrBitmap.Width - logoInnerSize) / 2;
                        int centerY = (qrBitmap.Height - logoInnerSize) / 2;

                        // Dibuja el logo respetando la transparencia
                        canvas.DrawBitmap(scaledLogo, centerX, centerY);

                        using var compositedImage = SKImage.FromBitmap(qrBitmap);
                        qrImageBytes = compositedImage.Encode(SKEncodedImageFormat.Png, 100).ToArray();
                    }
                    catch
                    {
                        // Si falla la superposición del logo, se devuelve el QR sin él.
                    }
                }

                // Devuelve un nuevo stream cada vez que MAUI necesite renderizar la imagen
                return ImageSource.FromStream(() => new MemoryStream(qrImageBytes));
            }
            catch (Exception)
            {
                return ImageSource.FromStream(() => Stream.Null);
            }
        }
    }
}
