using Android.Content;
using Android.Database;
using Android.Util;
using ApklisLicenseValidator.Apklis.Models;
using ApklisLicenseValidator.Apklis.Services;
using AndroidUri = Android.Net.Uri;

namespace ApklisLicenseValidator.Platforms.Android
{
    /// <summary>
    /// Implementación de <see cref="IApklisDataService"/> para Android.
    /// Obtiene los datos de la cuenta del usuario consultando el ContentProvider
    /// de la aplicación Apklis instalada en el dispositivo.
    /// </summary>
    public class ApklisDataGetter : IApklisDataService
    {
        private const string Tag = "ApklisDataGetter";
        private const string ContentAuthority = "cu.uci.android.apklis.ApklisLicenseProvider";

        private readonly AndroidUri _contentUri = AndroidUri.Parse($"content://{ContentAuthority}/account_data")!;
        private readonly Context _context = global::Android.App.Application.Context;

        // Nombres de columnas del ContentProvider de Apklis
        private const string ColumnUsername    = "username";
        private const string ColumnDeviceId    = "device_id";
        private const string ColumnAccessToken = "access_token";
        private const string ColumnCode        = "code";

        /// <inheritdoc />
        public ApklisAccountData? GetApklisAccountData()
        {
            ICursor? cursor = null;

            try
            {
                Log.Debug(Tag, "Consultando datos de cuenta de Apklis...");

                cursor = _context.ContentResolver!.Query(
                    _contentUri,
                    null, // proyección (todas las columnas)
                    null, // selección
                    null, // argumentos de selección
                    null  // orden
                );

                if (cursor == null)
                {
                    Log.Error(Tag, "El cursor es nulo. El ContentProvider de Apklis puede no estar disponible.");
                    return null;
                }

                Log.Debug(Tag, $"Cursor devuelto con {cursor.Count} filas.");

                if (cursor.MoveToFirst())
                {
                    var accountData = new ApklisAccountData(
                        username:    GetColumnValue(cursor, ColumnUsername),
                        deviceId:    GetColumnValue(cursor, ColumnDeviceId),
                        accessToken: GetColumnValue(cursor, ColumnAccessToken),
                        code:        GetColumnValue(cursor, ColumnCode));

                    Log.Debug(Tag, $"Datos recuperados: {accountData}");
                    return accountData;
                }

                Log.Warn(Tag, "El ContentProvider no devolvió datos de cuenta.");
                return null;
            }
            catch (Java.Lang.SecurityException ex)
            {
                Log.Error(Tag, $"SecurityException: permiso denegado para acceder al ContentProvider de Apklis. {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(Tag, $"Error al obtener los datos de cuenta de Apklis: {ex.Message}");
                return null;
            }
            finally
            {
                cursor?.Close();
            }
        }

        /// <inheritdoc />
        public bool IsApklisDataAvailable()
        {
            try
            {
                using var cursor = _context.ContentResolver!.Query(
                    _contentUri,
                    new[] { ColumnUsername }, // proyección mínima
                    null, null, null);

                return cursor != null;
            }
            catch (Exception ex)
            {
                Log.Debug(Tag, $"El proveedor de datos de Apklis no está disponible: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el valor de una columna del cursor de forma segura.
        /// Devuelve <c>null</c> si la columna no existe en el resultado.
        /// </summary>
        /// <param name="cursor">Cursor con los datos del ContentProvider.</param>
        /// <param name="columnName">Nombre de la columna a leer.</param>
        /// <returns>Valor de la columna o <c>null</c> si no existe.</returns>
        private static string? GetColumnValue(ICursor cursor, string columnName)
        {
            int columnIndex = cursor.GetColumnIndex(columnName);
            return columnIndex >= 0 ? cursor.GetString(columnIndex) : null;
        }
    }
}
