
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Servicio de verificación de firmas digitales RSA-SHA256.
    /// Utiliza la clave pública embebida como recurso del ensamblado para autenticar
    /// las respuestas de la API de Apklis y garantizar su integridad.
    /// </summary>
    public class SignatureVerificationService
    {
        private const string PublicKeyResourceName = "ApklisLicenseValidator.Resources.Raw.license_private_key.pub";

        /// <summary>
        /// Carga la clave pública en formato PEM desde los recursos embebidos del ensamblado.
        /// </summary>
        /// <returns>Contenido PEM de la clave pública, o <c>null</c> si no se pudo cargar.</returns>
        private string? LoadPublicKeyFromResources()
        {
            try
            {
                var assembly = typeof(SignatureVerificationService).GetTypeInfo().Assembly;

                using var stream = assembly.GetManifestResourceStream(PublicKeyResourceName);
                if (stream == null)
                {
                    Console.WriteLine("No se encontró el recurso de clave pública embebido.");
                    return null;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error al cargar la clave pública desde los recursos: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convierte una clave pública en formato PEM a su representación DER (bytes crudos).
        /// Elimina los encabezados/pies de página y decodifica el Base64 contenido.
        /// </summary>
        /// <param name="pemContent">Contenido de la clave en formato PEM.</param>
        /// <returns>Bytes de la clave en formato DER (SubjectPublicKeyInfo).</returns>
        private static byte[] ConvertPemToDer(string pemContent)
        {
            var base64Content = pemContent
                .Replace("-----BEGIN PUBLIC KEY-----", string.Empty)
                .Replace("-----END PUBLIC KEY-----", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);

            return Convert.FromBase64String(base64Content);
        }

        /// <summary>
        /// Verifica la firma digital RSA-SHA256 de un conjunto de datos usando
        /// la clave pública almacenada como recurso embebido.
        /// </summary>
        /// <param name="data">Datos originales cuya firma se desea verificar.</param>
        /// <param name="signatureBase64">Firma digital codificada en Base64.</param>
        /// <returns>
        /// <c>true</c> si la firma es válida para los datos proporcionados;
        /// <c>false</c> si la firma no es válida o si ocurrió algún error durante el proceso.
        /// </returns>
        public bool VerifySignature(byte[] data, string signatureBase64)
        {
            try
            {
                string? pemContent = LoadPublicKeyFromResources();
                if (string.IsNullOrEmpty(pemContent))
                    return false;

                byte[] publicKeyBytes = ConvertPemToDer(pemContent);
                byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                return rsa.VerifyData(data, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar la firma: {ex.Message}");
                return false;
            }
        }
    }
}