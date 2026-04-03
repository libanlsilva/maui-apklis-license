using System.Text.Json.Serialization;

namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Representa los datos del código QR generados por la API de Apklis
    /// para procesar un pago a través de Transfermóvil.
    /// </summary>
    public class QrCode
    {
        /// <summary>Identificador único de la transacción.</summary>
        [JsonPropertyName("id_transaccion")]
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>Importe a cobrar en la moneda indicada.</summary>
        [JsonPropertyName("importe")]
        public string Amount { get; set; } = string.Empty;

        /// <summary>Código de moneda (ej. CUP).</summary>
        [JsonPropertyName("moneda")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>Número identificador del proveedor del servicio.</summary>
        [JsonPropertyName("numero_proveedor")]
        public string ProviderNumber { get; set; } = string.Empty;

        /// <summary>Versión del protocolo QR utilizado.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Serializa el objeto a JSON usando las claves del protocolo Transfermóvil.
        /// Este método se utiliza para verificar la firma digital de la respuesta de la API.
        /// </summary>
        public string ToJsonString()
        {
            return $"{{\"id_transaccion\": \"{TransactionId}\", \"importe\": \"{Amount}\", \"moneda\": \"{Currency}\", \"numero_proveedor\": \"{ProviderNumber}\", \"version\": \"{Version}\"}}";
        }
    }
}
