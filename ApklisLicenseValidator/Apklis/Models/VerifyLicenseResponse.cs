using System.Text.Json.Serialization;

namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Representa la respuesta de la API de Apklis al verificar la licencia activa de un usuario.
    /// </summary>
    public class VerifyLicenseResponse
    {
        /// <summary>Fecha de expiración de la licencia en formato ISO 8601.</summary>
        [JsonPropertyName("expire_in")]
        public string ExpireIn { get; set; } = string.Empty;

        /// <summary>Identificador de la licencia activa del usuario.</summary>
        [JsonPropertyName("license")]
        public string License { get; set; } = string.Empty;

        /// <summary>
        /// Serializa el objeto a JSON en formato string.
        /// Se utiliza para la verificación de firma digital de la respuesta.
        /// </summary>
        public string ToJsonString()
        {
            return $"{{\"license\": \"{License}\", \"expire_in\": \"{ExpireIn}\"}}";
        }
    }
}
