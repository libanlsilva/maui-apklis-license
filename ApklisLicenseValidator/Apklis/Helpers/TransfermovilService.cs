using Android.Content;
using ApklisLicenseValidator.Apklis.Services;
using System.Diagnostics;

namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Implementación del servicio de pago mediante Transfermóvil para Android.
    /// Envía los datos del QR a la aplicación de Transfermóvil mediante un Intent
    /// y verifica el resultado del pago mediante polling a la API de Apklis.
    /// </summary>
    public class TransfermovilService : ITransfermovilService
    {
        private readonly ApklisValidatorService _apklisValidatorService;

        /// <summary>
        /// Crea una nueva instancia del servicio de Transfermóvil.
        /// </summary>
        /// <param name="apklisValidatorService">Servicio de validación de licencias de Apklis.</param>
        public TransfermovilService(ApklisValidatorService apklisValidatorService)
        {
            _apklisValidatorService = apklisValidatorService;
        }

        /// <inheritdoc />
        public async Task<bool> ProcessPaymentAsync(string qrData, string licenseId, string packageId)
        {
#if ANDROID
            // Envía los datos del QR a Transfermóvil mediante un Intent de tipo SEND
            var context = global::Android.App.Application.Context;
            var intent = new Intent(Intent.ActionSend);
            intent.PutExtra(Intent.ExtraText, qrData);
            intent.SetType("text/plain");
            intent.SetPackage("cu.etecsa.cubacel.tr.tm");
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
#endif
            // Inicia el proceso de polling para confirmar el pago
            return await PollForPaymentConfirmationAsync(packageId);
        }

        /// <summary>
        /// Consulta periódicamente la API de Apklis hasta confirmar el pago de la licencia
        /// o hasta que se alcance el tiempo máximo de espera.
        /// </summary>
        /// <param name="packageId">Nombre del paquete de la aplicación a verificar.</param>
        /// <returns><c>true</c> si el pago fue confirmado; <c>false</c> si expiró el tiempo de espera.</returns>
        private async Task<bool> PollForPaymentConfirmationAsync(string packageId)
        {
            const int pollingIntervalMs = 5_000;  // Intervalo entre consultas: 5 segundos
            const int maxTimeoutMs     = 90_000;  // Tiempo máximo de espera: 90 segundos

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < maxTimeoutMs)
            {
                try
                {
                    var verificationResult = await _apklisValidatorService.VerifyCurrentLicenseAsync(packageId);

                    if (verificationResult != null && string.IsNullOrEmpty(verificationResult.Error) && verificationResult.Paid)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error durante el polling de confirmación de pago: {ex.Message}");
                }

                await Task.Delay(pollingIntervalMs);
            }

            // Se alcanzó el tiempo máximo sin confirmar el pago
            return false;
        }
    }
}