namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Registra el resultado de una operación de pago o verificación de licencia en Apklis.
    /// Incluye el estado del pago, el usuario, la licencia activa (si corresponde),
    /// el código QR para pago (si corresponde) y el detalle del error (si lo hubiere).
    /// </summary>
    public class ApklisLicensePaymentStatus
    {
        /// <summary>
        /// El estado del pago (true o false).
        /// </summary>
        public bool Paid { get; }

        /// <summary>
        /// El nombre de usuario.
        /// </summary>
        public string? Username { get; }

        /// <summary>
        /// La licencia actual del usuario (si tiene).
        /// </summary>
        public string? License { get; }

        /// <summary>
        /// El error de la petición (si tiene).
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// El statusCode de error de la petición (si tiene).
        /// </summary>
        public int? StatusCode { get; set; }


        /// <summary>Fecha de expiración de la licencia activa (si está disponible).</summary>
        public DateTime? ExpireIn { get; }

        /// <summary>
        /// Crea una nueva instancia del resultado de pago/verificación.
        /// </summary>
        /// <param name="paid">Indica si la licencia está pagada.</param>
        /// <param name="username">Nombre de usuario que realizó la operación.</param>
        /// <param name="license">Identificador de la licencia activa (si tiene).</param>
        /// <param name="error">Mensaje de error (si ocurrió uno).</param>
        /// <param name="statusCode">Código HTTP de error de la API (si aplica).</param>
        /// <param name="expireIn">Fecha de expiración de la licencia.</param>
        /// <param name="qrCode">Datos del código QR para el pago por Transfermóvil.</param>
        public ApklisLicensePaymentStatus(
            bool paid = false,
            string? username = "",
            string? license = null,
            string? error = null,
            int? statusCode = null,
            DateTime? expireIn = null,
            QrCode? qrCode = null)
        {
            Paid = paid;
            Username = username;
            License = license;
            Error = error;
            StatusCode = statusCode;
            ExpireIn = expireIn;
            QrCode = qrCode;
        }

        /// <summary>Código QR generado para el pago por Transfermóvil (solo al comprar).</summary>
        public QrCode? QrCode { get; }
    }

}
