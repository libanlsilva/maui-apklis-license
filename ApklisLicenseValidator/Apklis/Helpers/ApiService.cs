using ApklisLicenseValidator.Apklis.Models;
using System.Text;
using System.Text.Json;

namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Servicio de acceso a la API REST de Apklis.
    /// Gestiona la comunicación HTTP con los endpoints de licencias.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private const int TimeoutSeconds = 30;
        private const string ApklisBaseUrl = "https://api.apklis.cu";
        private const string ApklisLicenseUrl = "license/v1/license";

        /// <summary>
        /// Crea una nueva instancia del servicio de API.
        /// </summary>
        /// <remarks>
        /// ADVERTENCIA DE SEGURIDAD: La validación del certificado SSL está deshabilitada.
        /// Esto es útil en entornos de desarrollo con certificados autofirmados, pero
        /// NO debe usarse en producción sin una justificación explícita.
        /// Para habilitar la validación, elimine la línea <c>ServerCertificateCustomValidationCallback</c>.
        /// </remarks>
        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                // TODO: Eliminar en producción si el servidor tiene un certificado SSL válido.
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Solicita el código QR de Transfermóvil para pagar una licencia.
        /// </summary>
        /// <param name="request">Solicitud de pago con el identificador de dispositivo.</param>
        /// <param name="licenseUuid">UUID de la licencia a pagar.</param>
        /// <param name="accessToken">Token JWT de autenticación del usuario.</param>
        /// <returns>Un <see cref="ApiResult{QrCode}"/> con el código QR o el error correspondiente.</returns>
        public async Task<ApiResult<QrCode>> PayLicenseWithTFAsync(PaymentRequest request, string licenseUuid, string accessToken)
        {
            try
            {
                string jsonBody = JsonSerializer.Serialize(request, _jsonOptions);
                var requestBody = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var requestUrl = $"{ApklisBaseUrl}/{ApklisLicenseUrl}/{licenseUuid}/pay-with-transfermovil/";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = requestBody
                };
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var qrCode = JsonSerializer.Deserialize<QrCode>(responseBody, _jsonOptions);
                    var headers = ConvertHeaders(response.Headers);

                    return new Success<QrCode>(qrCode!, headers);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return new Error<QrCode>((int)response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult<QrCode>(ex);
            }
        }

        /// <summary>
        /// Verifica si el usuario tiene una licencia activa para la aplicación indicada.
        /// </summary>
        /// <param name="request">Solicitud con el nombre del paquete y el identificador del dispositivo.</param>
        /// <param name="accessToken">Token JWT de autenticación del usuario.</param>
        /// <returns>Un <see cref="ApiResult{VerifyLicenseResponse}"/> con los datos de la licencia o el error.</returns>
        public async Task<ApiResult<VerifyLicenseResponse>> VerifyCurrentLicenseAsync(LicenseRequest request, string accessToken)
        {
            try
            {
                string jsonBody = JsonSerializer.Serialize(request, _jsonOptions);
                var requestBody = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var requestUrl = $"{ApklisBaseUrl}/{ApklisLicenseUrl}/verify/";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = requestBody
                };
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var licenseResponse = JsonSerializer.Deserialize<VerifyLicenseResponse>(responseBody, _jsonOptions);
                    var headers = ConvertHeaders(response.Headers);

                    return new Success<VerifyLicenseResponse>(licenseResponse!, headers);
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return new Error<VerifyLicenseResponse>((int)response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult<VerifyLicenseResponse>(ex);
            }
        }

        /// <summary>
        /// Convierte los encabezados HTTP de la respuesta en un diccionario de clave-valor.
        /// Cuando un encabezado tiene múltiples valores, se unen con coma.
        /// </summary>
        private static Dictionary<string, string> ConvertHeaders(System.Net.Http.Headers.HttpResponseHeaders headers)
        {
            var headerDictionary = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                headerDictionary[header.Key] = string.Join(",", header.Value);
            }
            return headerDictionary;
        }
    }
}
