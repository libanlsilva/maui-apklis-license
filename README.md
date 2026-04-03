# ApklisLicenseValidator (.NET MAUI)

Biblioteca en **.NET MAUI / C#** para la validación y compra de licencias a través de la plataforma [Apklis](https://www.apklis.cu). Proporciona los servicios necesarios para verificar si un usuario tiene una licencia activa y para iniciar el pago de una licencia mediante **Transfermóvil**.

> Este proyecto está basado en la biblioteca Kotlin original [z17-cuba/ApklisLicenseValidator](https://github.com/z17-cuba/ApklisLicenseValidator) y en el plugin Flutter equivalente, adaptados a .NET MAUI con C#.

---

## Requisitos

- .NET 9 + .NET MAUI
- Target platform: **Android** (API 21+)
- La aplicación **Apklis** instalada en el dispositivo del usuario

---

## Instalación

### 1. Copiar los archivos al proyecto

Copia la carpeta `Apklis/` al directorio raíz de tu proyecto MAUI. Contiene:

```
Apklis/
├── Helpers/
│   ├── ApiHelpers.cs
│   ├── ApiService.cs
│   ├── ITransfermovilService.cs
│   ├── LoggingHandler.cs
│   ├── SignatureVerificationService.cs
│   ├── ToolsHelper.cs
│   └── TransfermovilService.cs
├── Models/
│   ├── ApklisAccountData.cs
│   ├── ApklisLicensePaymentStatus.cs
│   ├── LicenseRequest.cs
│   ├── PaymentRequest.cs
│   ├── QrCode.cs
│   └── VerifyLicenseResponse.cs
└── Services/
    ├── ApklisValidatorService.cs
    ├── IApklisDataService.cs
    └── IPaymentResultCallback.cs
```

Además, copia el archivo `Platforms/Android/ApklisDataGetter.cs`.

### 2. Instalar dependencias NuGet

Añade los siguientes paquetes NuGet a tu proyecto:

```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
<PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="3.116.1" />
```

### 3. Colocar la clave pública de cifrado

La clave pública generada para tu grupo de licencias debe colocarse como recurso embebido en:

```
Resources/Raw/license_private_key.pub
```

En el archivo `.csproj`, asegúrate de que el archivo esté configurado como `EmbeddedResource`:

```xml
<ItemGroup>
    <EmbeddedResource Include="Resources\Raw\license_private_key.pub" />
</ItemGroup>
```

> ⚠️ **Cada desarrollador recibe una clave única** asociada a su grupo de licencias en Apklis. Sin esta clave, la verificación de firma de las respuestas de la API fallará.

---

## Configuración

### Registro de servicios en `MauiProgram.cs`

Registra todos los servicios en el contenedor de inyección de dependencias de MAUI:

```csharp
using ApklisLicenseValidator.Apklis.Helpers;
using ApklisLicenseValidator.Apklis.Services;
using ApklisLicenseValidator.Platforms.Android;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Registro de servicios de Apklis
        builder.Services.AddSingleton<IApklisDataService, ApklisDataGetter>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<ApklisValidatorService>();
        builder.Services.AddSingleton<ITransfermovilService, TransfermovilService>();

        return builder.Build();
    }
}
```

### Permisos en `AndroidManifest.xml`

Añade los permisos requeridos en `Platforms/Android/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.GET_ACCOUNTS" />
<uses-permission android:name="cu.uci.android.apklis.READ_ACCOUNT_DATA" />
```

---

## Clase `ApklisLicensePaymentStatus`

Todas las operaciones devuelven un objeto `ApklisLicensePaymentStatus` con el resultado de la operación.

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Paid` | `bool` | Indica si el usuario tiene una licencia activa o si el pago fue confirmado |
| `Username` | `string?` | Nombre de usuario en Apklis |
| `License` | `string?` | Identificador de la licencia activa |
| `Error` | `string?` | Mensaje de error (nulo si la operación fue exitosa) |
| `StatusCode` | `int?` | Código HTTP de error de la API (si aplica) |
| `ExpireIn` | `DateTime?` | Fecha de expiración de la licencia |
| `QrCode` | `QrCode?` | Datos del QR para pago por Transfermóvil (solo en `PurchaseLicenseAsync`) |

---

## Uso

### Inyección de dependencias (recomendado)

```csharp
public class MyPage : ContentPage
{
    private readonly ApklisValidatorService _licenseValidator;
    private readonly ITransfermovilService _transfermovilService;

    public MyPage(ApklisValidatorService licenseValidator, ITransfermovilService transfermovilService)
    {
        _licenseValidator = licenseValidator;
        _transfermovilService = transfermovilService;
    }
}
```

---

### 1. Verificar licencia activa

Comprueba si el usuario tiene una licencia activa para el paquete indicado.

```csharp
private async Task CheckLicenseAsync()
{
    var result = await _licenseValidator.VerifyCurrentLicenseAsync("com.example.myapp");

    if (string.IsNullOrEmpty(result.Error))
    {
        if (result.Paid)
        {
            // Licencia válida encontrada
            Console.WriteLine($"Usuario: {result.Username}");
            Console.WriteLine($"Licencia: {result.License}");
            Console.WriteLine($"Expira: {result.ExpireIn}");
            EnablePremiumFeatures();
        }
        else
        {
            // No tiene licencia activa
            DisablePremiumFeatures();
        }
    }
    else
    {
        // Error en la verificación
        Console.WriteLine($"Error: {result.Error}");

        switch (result.StatusCode)
        {
            case 402:
                // El usuario debe comprar una licencia
                break;
            case 403:
                // Credenciales expiradas — pedir al usuario que abra Apklis y se autentique
                break;
            case 404:
                // El grupo de licencias no ha sido publicado aún
                break;
        }
    }
}
```

---

### 2. Comprar una licencia

Inicia el proceso de compra de una licencia. Si la API responde con éxito, se obtiene el código QR de Transfermóvil para completar el pago.

```csharp
private async Task PurchaseLicenseAsync()
{
    // Paso 1: Obtener el código QR de pago desde la API de Apklis
    var result = await _licenseValidator.PurchaseLicenseAsync("your-license-uuid");

    if (string.IsNullOrEmpty(result.Error))
    {
        if (result.QrCode != null)
        {
            // Paso 2: Mostrar el QR al usuario
            QrImageControl.Source = ToolsHelper.GenerateQr(result.QrCode);

            string qrJson = result.QrCode.ToJsonString();

            // Paso 3: Enviar el QR a la app de Transfermóvil y esperar confirmación
            bool paymentConfirmed = await _transfermovilService.ProcessPaymentAsync(
                qrData: qrJson,
                licenseId: "your-license-uuid",
                packageId: "com.example.myapp");

            if (paymentConfirmed)
            {
                await DisplayAlert("Pago", "¡Pago confirmado!", "OK");
                EnablePremiumFeatures();
            }
            else
            {
                await DisplayAlert("Pago", "Pago no confirmado o expiró el tiempo de espera.", "OK");
            }
        }
    }
    else
    {
        Console.WriteLine($"Error al iniciar compra: {result.Error}");

        switch (result.StatusCode)
        {
            case 400:
                // Error de pago (usualmente timeout). Reintentar resuelve el problema.
                break;
            case 403:
                // Credenciales expiradas — pedir al usuario que abra Apklis y se autentique
                break;
        }
    }
}
```

---

### 3. Generar imagen del QR (opcional)

`ToolsHelper.GenerateQr()` genera un `ImageSource` listo para usar en cualquier control `Image` de MAUI. Si existe el recurso `apklist.png` embebido, superpone el logotipo en el centro del QR.

```csharp
// Genera el QR como ImageSource (admite logotipo embebido)
ImageSource qrImage = ToolsHelper.GenerateQr(result.QrCode);
MyQrImageControl.Source = qrImage;
```

---

## Códigos de estado HTTP

### `VerifyCurrentLicenseAsync`

| Código | Descripción |
|--------|-------------|
| `402` | El usuario debe pagar una licencia para la aplicación |
| `403` | Credenciales no reconocidas o expiradas. Se recomienda que el usuario abra Apklis y se autentique |
| `404` | El grupo de licencias no ha sido publicado aún |

### `PurchaseLicenseAsync`

| Código | Descripción |
|--------|-------------|
| `400` | Error en el proceso de pago (usualmente timeout a la API). Reintentar suele resolver el problema |
| `403` | Credenciales no reconocidas o expiradas. Se recomienda que el usuario abra Apklis y se autentique |

---

## Estructura del proyecto

```
Apklis/
├── Helpers/
│   ├── ApiHelpers.cs               — Tipos resultado de API: Success<T>, Error<T>, ExceptionResult<T>
│   ├── ApiService.cs               — Cliente HTTP para los endpoints de licencias de Apklis
│   ├── ITransfermovilService.cs    — Contrato del servicio de pago por Transfermóvil
│   ├── LoggingHandler.cs           — Manejador HTTP que registra solicitudes/respuestas (debug)
│   ├── SignatureVerificationService.cs — Verificación de firma RSA-SHA256 de respuestas de la API
│   ├── ToolsHelper.cs              — Utilidades: texto de expiración, generación de imagen QR
│   └── TransfermovilService.cs     — Envío del QR a Transfermóvil + polling de confirmación
├── Models/
│   ├── ApklisAccountData.cs        — Datos de cuenta del usuario (username, deviceId, token)
│   ├── ApklisLicensePaymentStatus.cs — Resultado de verificación o compra de licencia
│   ├── LicenseRequest.cs           — Request de verificación (packageName + deviceId)
│   ├── PaymentRequest.cs           — Request de pago (deviceId)
│   ├── QrCode.cs                   — Datos del QR de Transfermóvil
│   └── VerifyLicenseResponse.cs    — Respuesta de la API al verificar licencia
└── Services/
    ├── ApklisValidatorService.cs   — Servicio principal: PurchaseLicenseAsync + VerifyCurrentLicenseAsync
    ├── IApklisDataService.cs       — Contrato del proveedor de datos de cuenta de Apklis
    └── IPaymentResultCallback.cs   — Callbacks de resultado de pago (completado, fallido, cerrado)

Platforms/Android/
└── ApklisDataGetter.cs             — Implementación Android: lee datos del ContentProvider de Apklis
```

---

## FAQs — Errores conocidos

### Error 403 repetido con credenciales válidas

Reportado en dispositivos **Xiaomi Redmi Note 11 con Android 11** y otros. Si agotaste las siguientes opciones:

1. Iniciar sesión en Apklis
2. Realizar alguna acción para refrescar el token expirado
3. Cerrar sesión y volver a iniciar
4. Asegurarte de que Apklis esté activo en segundo plano
5. Verificar en **Ajustes → Cuentas y sincronización** que la cuenta de Apklis se creó correctamente

Entonces añade esta línea al `AndroidManifest.xml` de tu aplicación:

```xml
<queries>
    <package android:name="cu.uci.android.apklis" />
</queries>
```

---

### Error 403 en Android 16

Aplica al mismo problema pero en dispositivos con **Android 16**. Probado con Google Pixel 8 y Samsung S24:

```xml
<queries>
    <intent>
        <action android:name="android.accounts.AccountAuthenticator" />
    </intent>
    <intent>
        <action android:name="android.intent.action.VIEW" />
        <data android:scheme="apklis" />
    </intent>
</queries>
```

---

## Notas de seguridad

- La validación del certificado SSL está **deshabilitada por defecto** en `ApiService.cs` para compatibilidad con el entorno de Apklis. Revisa si esto aplica a tu caso de uso en producción.
- La clave pública `license_private_key.pub` se embebe en el binario de la app. **No la compartas ni la publiques en repositorios públicos.**
- La firma digital RSA-SHA256 de cada respuesta de la API se verifica automáticamente antes de dar por válido cualquier resultado.

---

## Créditos

Basado en el trabajo original de:
- [z17-cuba/ApklisLicenseValidator](https://github.com/z17-cuba/ApklisLicenseValidator) — biblioteca Kotlin nativa para Android
- Plugin Flutter equivalente para la plataforma Apklis
