namespace ApklisLicenseValidator.Apklis.Services
{
    /// <summary>
    /// Define los callbacks para notificar el resultado de un pago procesado a través de Transfermóvil.
    /// Implemente esta interfaz para reaccionar al éxito, el fallo o el cierre del diálogo de pago.
    /// </summary>
    public interface IPaymentResultCallback
    {
        /// <summary>
        /// Se invoca cuando el pago se completó exitosamente.
        /// </summary>
        /// <param name="licenseName">Nombre o identificador de la licencia activada.</param>
        void OnPaymentCompleted(string licenseName);

        /// <summary>
        /// Se invoca cuando el pago falló.
        /// </summary>
        /// <param name="error">Descripción del error ocurrido.</param>
        void OnPaymentFailed(string error);

        /// <summary>
        /// Se invoca cuando el usuario cierra el diálogo de pago sin completar la transacción.
        /// </summary>
        void OnDialogClosed();
    }
}
