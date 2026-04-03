using ApklisLicenseValidator.Apklis.Helpers;
using ApklisLicenseValidator.Apklis.Services;

namespace ApklisLicenseValidator
{
    /// <summary>
    /// Página principal de demostración del uso de la biblioteca ApklisLicenseValidator.
    /// Permite verificar el estado de una licencia y realizar el pago a través de Transfermóvil.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly ApklisValidatorService _licenseValidator;
        private readonly ITransfermovilService _transfermovilService;

        /// <summary>
        /// Crea la página principal inyectando los servicios necesarios.
        /// </summary>
        /// <param name="licenseValidator">Servicio de validación y compra de licencias.</param>
        /// <param name="transfermovilService">Servicio de pago mediante Transfermóvil.</param>
        public MainPage(ApklisValidatorService licenseValidator, ITransfermovilService transfermovilService)
        {
            InitializeComponent();
            _licenseValidator = licenseValidator;
            _transfermovilService = transfermovilService;
        }

        /// <summary>
        /// Verifica el estado de la licencia del usuario para el Package ID ingresado.
        /// </summary>
        private async void OnCheckLicenseClicked(object sender, EventArgs e)
        {
            SetLoading(true);

            try
            {
                string? packageId = PackageIdEntry.Text?.Trim();

                if (string.IsNullOrEmpty(packageId))
                {
                    await DisplayAlert("Error", "Por favor ingrese un Package ID válido.", "OK");
                    return;
                }

                var result = await _licenseValidator.VerifyCurrentLicenseAsync(packageId);

                if (string.IsNullOrEmpty(result.Error))
                {
                    UsernameLabel.Text = result.Username ?? "-";
                    LicenseLabel.Text  = result.License  ?? "-";
                    PaidLabel.Text     = result.Paid ? "Sí" : "No";
                    ExpireInLabel.Text = result.ExpireIn.HasValue
                        ? ToolsHelper.GetLicenseExpirationText(result.ExpireIn.Value)
                        : "-";

                    fError.IsVisible = false;
                    fOk.IsVisible    = true;
                }
                else
                {
                    EUsernameLabel.Text = result.Username ?? "-";
                    ErorLabel.Text      = result.Error    ?? "-";
                    fOk.IsVisible       = false;
                    fError.IsVisible    = true;
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Inicia el proceso de pago de la licencia cuyo ID se ha ingresado,
        /// utilizando Transfermóvil como pasarela de pago.
        /// </summary>
        private async void OnPayLicenseClicked(object sender, EventArgs e)
        {
            SetLoading(true);

            try
            {
                string? licenseId = LicenseIdToPayEntry.Text?.Trim();
                string? packageId = PackageIdEntry.Text?.Trim();

                if (string.IsNullOrEmpty(licenseId))
                {
                    await DisplayAlert("Error", "Por favor, ingrese el ID de la licencia.", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(packageId))
                {
                    await DisplayAlert("Error", "Por favor ingrese un Package ID válido.", "OK");
                    return;
                }

                var result = await _licenseValidator.PurchaseLicenseAsync(licenseId);

                if (string.IsNullOrEmpty(result.Error))
                {
                    if (result.QrCode != null)
                    {
                        QrImage.Source = ToolsHelper.GenerateQr(result.QrCode);

                        string qrJson = result.QrCode.ToJsonString();
                        bool paymentSucceeded = await _transfermovilService.ProcessPaymentAsync(qrJson, licenseId, packageId);

                        if (paymentSucceeded)
                            await DisplayAlert("Pago", "¡Pago confirmado!", "OK");
                        else
                            await DisplayAlert("Pago", "Pago no confirmado o fallido.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo generar el código QR para el pago.", "OK");
                    }
                }
                else
                {
                    PEUsernameLabel.Text = result.Username ?? "-";
                    PErorLabel.Text      = result.Error    ?? "-";
                    fPError.IsVisible    = true;
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Muestra u oculta el indicador de carga global en la pantalla.
        /// </summary>
        /// <param name="isLoading"><c>true</c> para mostrar el indicador; <c>false</c> para ocultarlo.</param>
        private void SetLoading(bool isLoading)
        {
            LoadingOverlay.IsVisible      = isLoading;
            GlobalLoadingIndicator.IsRunning = isLoading;
        }
    }
}
