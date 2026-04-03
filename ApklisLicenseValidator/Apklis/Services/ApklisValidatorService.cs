using Android.Util;
using ApklisLicenseValidator.Apklis.Helpers;
using ApklisLicenseValidator.Apklis.Models;

namespace ApklisLicenseValidator.Apklis.Services
{
    /// <summary>
    /// Servicio principal de validación y compra de licencias de Apklis.
    /// Coordina las llamadas a la API REST, la verificación de firmas digitales
    /// y la construcción del resultado de cada operación.
    /// </summary>
    public class ApklisValidatorService
    {
        private const string LogTag = "ApklisValidator";
        private const string SignatureHeaderName = "signature";

        private readonly SignatureVerificationService _signatureService;
        private readonly IApklisDataService _apklisDataService;
        private readonly ApiService _apiService;

        /// <summary>
        /// Crea una nueva instancia del servicio validador de licencias.
        /// </summary>
        /// <param name="apklisDataService">Servicio que provee los datos de la cuenta del usuario.</param>
        /// <param name="apiService">Servicio HTTP para comunicarse con la API de Apklis.</param>
        public ApklisValidatorService(IApklisDataService apklisDataService, ApiService apiService)
        {
            _apklisDataService = apklisDataService;
            _apiService = apiService;
            _signatureService = new SignatureVerificationService();
        }

        /// <summary>
        /// Inicia el proceso de compra de una licencia a través de Transfermóvil.
        /// Obtiene el código QR de pago desde la API de Apklis y verifica la firma
        /// de la respuesta antes de devolverla al llamador.
        /// </summary>
        /// <param name="licenseUuid">UUID de la licencia a adquirir.</param>
        /// <returns>
        /// Un <see cref="ApklisLicensePaymentStatus"/> con el código QR si fue exitoso,
        /// o con el detalle del error si ocurrió algún problema.
        /// </returns>
        public async Task<ApklisLicensePaymentStatus> PurchaseLicenseAsync(string licenseUuid)
        {
            ApklisAccountData accountData = _apklisDataService.GetApklisAccountData()
                ?? new ApklisAccountData();

            try
            {
                var paymentResult = await _apiService.PayLicenseWithTFAsync(
                    new PaymentRequest(accountData.DeviceId ?? string.Empty),
                    licenseUuid,
                    accountData.AccessToken ?? string.Empty);

                if (paymentResult is Success<QrCode> success)
                {
                    bool isSignatureValid = VerifySignatureIfPresent(
                        success.Data.ToJsonString(),
                        success.Headers);

                    if (!isSignatureValid)
                    {
                        Log.Warn(LogTag, "La verificación de firma falló para la respuesta de compra.");
                        return new ApklisLicensePaymentStatus(
                            error: "Firma de respuesta inválida.",
                            username: accountData.Username ?? string.Empty);
                    }

                    return new ApklisLicensePaymentStatus(qrCode: success.Data, username: accountData.Username ?? string.Empty);
                }
                else if (paymentResult is Error<QrCode> error)
                {
                    string errorMessage = $"Error al efectuar el pago {error.Code}: {error.Message}";
                    Log.Error(LogTag, errorMessage);
                    return new ApklisLicensePaymentStatus(
                        error: error.Message,
                        statusCode: error.Code,
                        username: accountData.Username ?? string.Empty);
                }
                else if (paymentResult is ExceptionResult<QrCode> exceptionResult)
                {
                    string errorMessage = $"Excepción al efectuar el pago: {exceptionResult.Throwable.Message}";
                    Log.Error(LogTag, errorMessage);
                    return new ApklisLicensePaymentStatus(
                        error: errorMessage,
                        username: accountData.Username ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogTag, $"Error inesperado en PurchaseLicenseAsync: {ex.Message}");
                return new ApklisLicensePaymentStatus(
                    error: ex.Message,
                    username: accountData.Username ?? string.Empty);
            }

            return new ApklisLicensePaymentStatus(
                error: "Resultado de pago desconocido.",
                username: accountData.Username ?? string.Empty);
        }

        /// <summary>
        /// Verifica si el usuario tiene una licencia activa para la aplicación indicada.
        /// Consulta la API de Apklis y valida la firma digital de la respuesta.
        /// </summary>
        /// <param name="packageId">Nombre del paquete de la aplicación (ej. com.example.myapp).</param>
        /// <returns>
        /// Un <see cref="ApklisLicensePaymentStatus"/> con el estado de la licencia,
        /// o con el detalle del error si ocurrió algún problema.
        /// </returns>
        public async Task<ApklisLicensePaymentStatus> VerifyCurrentLicenseAsync(string packageId)
        {
            ApklisAccountData accountData = _apklisDataService.GetApklisAccountData()
                ?? new ApklisAccountData();

            try
            {
                var verificationResult = await _apiService.VerifyCurrentLicenseAsync(
                    new LicenseRequest(packageId, accountData.DeviceId ?? string.Empty),
                    accountData.AccessToken ?? string.Empty);

                if (verificationResult is Success<VerifyLicenseResponse> success)
                {
                    bool isSignatureValid = VerifySignatureIfPresent(
                        success.Data.ToJsonString(),
                        success.Headers);

                    if (!isSignatureValid)
                    {
                        Log.Warn(LogTag, "La verificación de firma falló para la respuesta de licencia.");
                        return new ApklisLicensePaymentStatus(
                            paid: false,
                            error: "Firma de respuesta inválida.",
                            username: accountData.Username ?? string.Empty);
                    }

                    bool hasPaidLicense = !string.IsNullOrEmpty(success.Data.License);
                    DateTime? expirationDate = DateTime.TryParse(success.Data.ExpireIn, out var parsedDate)
                        ? parsedDate
                        : (DateTime?)null;

                    return new ApklisLicensePaymentStatus(
                        paid: hasPaidLicense,
                        license: success.Data.License,
                        username: accountData.Username ?? string.Empty,
                        expireIn: expirationDate);
                }
                else if (verificationResult is Error<VerifyLicenseResponse> error)
                {
                    string errorMessage = $"Fallo al efectuar la verificación {error.Code}: {error.Message}";
                    Log.Error(LogTag, errorMessage);
                    return new ApklisLicensePaymentStatus(
                        error: error.Message,
                        statusCode: error.Code,
                        username: accountData.Username ?? string.Empty);
                }
                else if (verificationResult is ExceptionResult<VerifyLicenseResponse> exceptionResult)
                {
                    string errorMessage = $"Excepción al verificar licencia: {exceptionResult.Throwable.Message}";
                    Log.Error(LogTag, errorMessage);
                    return new ApklisLicensePaymentStatus(
                        error: errorMessage,
                        username: accountData.Username ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogTag, $"Error inesperado en VerifyCurrentLicenseAsync: {ex.Message}");
                return new ApklisLicensePaymentStatus(
                    error: ex.Message,
                    username: accountData.Username ?? string.Empty);
            }

            return new ApklisLicensePaymentStatus(
                error: "Resultado de verificación desconocido.",
                username: accountData.Username ?? string.Empty);
        }

        /// <summary>
        /// Verifica la firma digital presente en los encabezados de la respuesta HTTP.
        /// Si el encabezado de firma no está presente, se considera la respuesta inválida.
        /// </summary>
        /// <param name="responseBody">Cuerpo de la respuesta serializado como JSON.</param>
        /// <param name="headers">Encabezados HTTP de la respuesta.</param>
        /// <returns><c>true</c> si la firma es válida; <c>false</c> en caso contrario.</returns>
        private bool VerifySignatureIfPresent(string responseBody, IDictionary<string, string>? headers)
        {
            if (headers == null ||
                !headers.TryGetValue(SignatureHeaderName, out var signatureValue) ||
                string.IsNullOrEmpty(signatureValue))
            {
                Log.Warn(LogTag, "No se encontró el encabezado de firma en la respuesta.");
                return false;
            }

            return _signatureService.VerifySignature(
                System.Text.Encoding.UTF8.GetBytes(responseBody),
                signatureValue);
        }
    }
}
