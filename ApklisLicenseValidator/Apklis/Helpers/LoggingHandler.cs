using System.Diagnostics;

namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Manejador de mensajes HTTP que registra en consola los detalles de cada
    /// solicitud y respuesta para facilitar la depuración.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// Crea una nueva instancia del manejador de logging.
        /// </summary>
        /// <param name="innerHandler">Manejador HTTP interno que procesará las solicitudes.</param>
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler) { }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[REQUEST] {request.Method} {request.RequestUri}");
            Console.WriteLine($"[HEADERS] {request.Headers}");

            var stopwatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            Console.WriteLine($"[RESPONSE] {(int)response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");
            Console.WriteLine($"[RESPONSE HEADERS] {response.Headers}");

            return response;
        }
    }
}
