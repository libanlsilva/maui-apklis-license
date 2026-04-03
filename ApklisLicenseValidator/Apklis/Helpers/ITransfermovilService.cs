namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Define el contrato para procesar pagos a través de la aplicación Transfermóvil.
    /// </summary>
    public interface ITransfermovilService
    {
        /// <summary>
        /// Inicia el proceso de pago enviando los datos QR a Transfermóvil y espera
        /// la confirmación del pago mediante polling al servidor de Apklis.
        /// </summary>
        /// <param name="qrData">JSON con los datos del código QR de Transfermóvil.</param>
        /// <param name="licenseId">Identificador UUID de la licencia a pagar.</param>
        /// <param name="packageId">Nombre del paquete de la aplicación.</param>
        /// <returns><c>true</c> si el pago fue confirmado dentro del tiempo límite; de lo contrario, <c>false</c>.</returns>
        Task<bool> ProcessPaymentAsync(string qrData, string licenseId, string packageId);
    }
}
