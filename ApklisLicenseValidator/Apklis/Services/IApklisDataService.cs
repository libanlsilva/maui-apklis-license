using ApklisLicenseValidator.Apklis.Models;

namespace ApklisLicenseValidator.Apklis.Services
{
    /// <summary>
    /// Define el contrato para obtener los datos de la cuenta de Apklis del usuario.
    /// La implementación accede al ContentProvider de la aplicación Apklis instalada en el dispositivo.
    /// </summary>
    public interface IApklisDataService
    {
        /// <summary>
        /// Obtiene los datos de la cuenta de Apklis del usuario actual.
        /// </summary>
        /// <returns>Un objeto <see cref="ApklisAccountData"/> con los datos del usuario, o <c>null</c> si no están disponibles.</returns>
        ApklisAccountData? GetApklisAccountData();

        /// <summary>
        /// Verifica si el proveedor de datos de Apklis está disponible en el dispositivo.
        /// </summary>
        /// <returns><c>true</c> si Apklis está instalado y el proveedor responde; de lo contrario, <c>false</c>.</returns>
        bool IsApklisDataAvailable();
    }
}
