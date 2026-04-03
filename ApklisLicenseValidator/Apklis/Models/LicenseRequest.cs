using System.Text.Json.Serialization;

namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Modelo de solicitud para verificar la licencia activa de una aplicación.
    /// Se envía al endpoint de verificación de la API de Apklis.
    /// </summary>
    public class LicenseRequest
    {
        /// <summary>Nombre del paquete de la aplicación (ej. com.example.myapp).</summary>
        [JsonPropertyName("package_name")]
        public string PackageName { get; set; } = string.Empty;

        /// <summary>Identificador único del dispositivo.</summary>
        [JsonPropertyName("device")]
        public string Device { get; set; } = string.Empty;

        /// <summary>Constructor sin parámetros requerido para la deserialización JSON.</summary>
        public LicenseRequest() { }

        /// <summary>
        /// Crea una nueva solicitud de verificación de licencia.
        /// </summary>
        /// <param name="packageName">Nombre del paquete de la aplicación.</param>
        /// <param name="device">Identificador único del dispositivo.</param>
        public LicenseRequest(string packageName, string device)
        {
            PackageName = packageName;
            Device = device;
        }
    }
}
