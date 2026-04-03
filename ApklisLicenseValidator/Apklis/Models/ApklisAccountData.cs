namespace ApklisLicenseValidator.Apklis.Models
{
    /// <summary>
    /// Contiene los datos de la cuenta del usuario en Apklis, obtenidos
    /// a través del ContentProvider del sistema.
    /// </summary>
    public class ApklisAccountData
    {
        /// <summary>Nombre de usuario en Apklis.</summary>
        public string? Username { get; set; }

        /// <summary>Identificador único del dispositivo Android.</summary>
        public string? DeviceId { get; set; }

        /// <summary>Token de acceso (JWT) para autenticarse en la API de Apklis.</summary>
        public string? AccessToken { get; set; }

        /// <summary>Código de verificación asociado a la cuenta.</summary>
        public string? Code { get; set; }

        /// <summary>Constructor sin parámetros requerido para la deserialización.</summary>
        public ApklisAccountData() { }

        /// <summary>
        /// Crea una nueva instancia con todos los datos de la cuenta.
        /// </summary>
        /// <param name="username">Nombre de usuario en Apklis.</param>
        /// <param name="deviceId">Identificador único del dispositivo.</param>
        /// <param name="accessToken">Token JWT de acceso a la API.</param>
        /// <param name="code">Código de verificación de la cuenta.</param>
        public ApklisAccountData(string? username, string? deviceId, string? accessToken, string? code)
        {
            Username = username;
            DeviceId = deviceId;
            AccessToken = accessToken;
            Code = code;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ApklisAccountData(" +
                   $"username='{Username}', " +
                   $"deviceId='{(DeviceId != null ? "***" : "null")}', " +
                   $"accessToken='{(AccessToken != null ? "***" : "null")}', " +
                   $"code='{(Code != null ? "***" : "null")}')";
        }
    }
}
