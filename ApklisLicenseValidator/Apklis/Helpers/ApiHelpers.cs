namespace ApklisLicenseValidator.Apklis.Helpers
{
    /// <summary>
    /// Tipo base del patrón de resultado discriminado que envuelve las respuestas de la API.
    /// Permite manejar éxito, error HTTP y excepciones de forma tipada y segura.
    /// </summary>
    public abstract class ApiResult<T> { }

    /// <summary>
    /// Resultado exitoso de una llamada a la API.
    /// </summary>
    public class Success<T> : ApiResult<T>
    {
        /// <summary>Datos deserializados de la respuesta.</summary>
        public T Data { get; }

        /// <summary>Encabezados HTTP de la respuesta (pueden incluir la firma digital).</summary>
        public Dictionary<string, string>? Headers { get; }

        /// <summary>
        /// Crea un resultado exitoso.
        /// </summary>
        /// <param name="data">Datos de la respuesta.</param>
        /// <param name="headers">Encabezados HTTP de la respuesta.</param>
        public Success(T data, Dictionary<string, string>? headers = null)
        {
            Data = data;
            Headers = headers;
        }
    }

    /// <summary>
    /// Resultado de error HTTP de una llamada a la API (código de estado no exitoso).
    /// </summary>
    public class Error<T> : ApiResult<T>
    {
        /// <summary>Código de estado HTTP del error.</summary>
        public int? Code { get; }

        /// <summary>Cuerpo de la respuesta de error.</summary>
        public string Message { get; }

        /// <summary>Encabezados HTTP de la respuesta de error.</summary>
        public Dictionary<string, string>? Headers { get; }

        /// <summary>
        /// Crea un resultado de error HTTP.
        /// </summary>
        /// <param name="code">Código de estado HTTP.</param>
        /// <param name="message">Cuerpo del mensaje de error.</param>
        /// <param name="headers">Encabezados HTTP de la respuesta.</param>
        public Error(int? code, string message, Dictionary<string, string>? headers = null)
        {
            Code = code;
            Message = message;
            Headers = headers;
        }
    }

    /// <summary>
    /// Resultado de excepción producido cuando la llamada a la API lanza una excepción inesperada.
    /// </summary>
    public class ExceptionResult<T> : ApiResult<T>
    {
        /// <summary>Excepción capturada durante la llamada a la API.</summary>
        public Exception Throwable { get; }

        /// <summary>
        /// Crea un resultado de excepción.
        /// </summary>
        /// <param name="throwable">Excepción capturada.</param>
        public ExceptionResult(Exception throwable)
        {
            Throwable = throwable;
        }
    }
}
