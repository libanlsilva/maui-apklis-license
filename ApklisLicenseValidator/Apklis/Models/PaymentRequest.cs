using System.Text.Json.Serialization;

namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Modelo de solicitud para iniciar el pago de una licencia a través de Transfermóvil.
    /// Se envía al endpoint de pago de la API de Apklis.
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>Identificador único del dispositivo que realiza el pago.</summary>
        [JsonPropertyName("device")]
        public string Device { get; set; } = string.Empty;

        /// <summary>Constructor sin parámetros requerido para la deserialización JSON.</summary>
        public PaymentRequest() { }

        /// <summary>
        /// Crea una nueva solicitud de pago.
        /// </summary>
        /// <param name="device">Identificador único del dispositivo.</param>
        public PaymentRequest(string device)
        {
            Device = device;
        }
    }
}
