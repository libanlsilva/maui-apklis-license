using ApklisLicenseValidator.Apklis.Helpers;
using ApklisLicenseValidator.Apklis.Services;
using ApklisLicenseValidator.Platforms.Android;
using Microsoft.Extensions.Logging;

namespace ApklisLicenseValidator
{
    /// <summary>
    /// Punto de entrada de la aplicación MAUI.
    /// Configura el contenedor de inyección de dependencias y los servicios necesarios
    /// para la validación de licencias de Apklis.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Crea y configura la aplicación MAUI.
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registro de servicios de Apklis
            builder.Services.AddSingleton<IApklisDataService, ApklisDataGetter>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<ApklisValidatorService>();
            builder.Services.AddSingleton<ITransfermovilService, TransfermovilService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
