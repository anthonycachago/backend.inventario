using System.Net;
using System.Net.Http.Json;
using Inventario.Web.Entity;
using Microsoft.Extensions.Caching.Memory;

namespace Inventario.Web.Services.Sri;

/// <summary>Datos consolidados de una consulta al SRI, listos para llenar un formulario.</summary>
public record ResultadoSri(
    string Identificacion,
    TipoIdentificacion Tipo,
    string Nombre,
    string? Direccion,
    string? Estado,
    string? Actividad,
    string? TipoContribuyente,
    string? Regimen,
    IReadOnlyList<string> Alertas)
{
    public bool Activo => Estado is null || Estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase);
}

public class SriNoDisponibleException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Consulta el catastro público del SRI (los mismos servicios que usa srienlinea.sri.gob.ec).
/// No es una API documentada: si el SRI la cambia, la app sigue funcionando y el usuario ingresa los datos a mano.
/// </summary>
public class SriService(HttpClient http, IMemoryCache cache, ILogger<SriService> logger)
{
    public const string UrlBase = "https://srienlinea.sri.gob.ec/sri-catastro-sujeto-servicio-internet/rest/";

    /// <summary>Consulta por RUC (13 dígitos) o cédula (10). Devuelve null si el SRI no tiene registro.</summary>
    public async Task<ResultadoSri?> ConsultarAsync(string identificacion, CancellationToken ct = default)
    {
        identificacion = identificacion.Trim();
        var tipo = IdentificacionEcuador.Deducir(identificacion);
        if (tipo is not (TipoIdentificacion.Cedula or TipoIdentificacion.Ruc))
            throw new ReglaNegocioException("Ingrese una cédula (10 dígitos) o un RUC (13 dígitos).");
        if (IdentificacionEcuador.Validar(tipo.Value, identificacion) is { } error)
            throw new ReglaNegocioException(error);

        var clave = $"sri:{identificacion}";
        if (cache.TryGetValue(clave, out ResultadoSri? enCache)) return enCache;

        ResultadoSri? resultado;
        try
        {
            resultado = tipo == TipoIdentificacion.Ruc
                ? await ConsultarRucAsync(identificacion, ct)
                : await ConsultarCedulaAsync(identificacion, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Consulta al SRI fallida para {Identificacion}", identificacion);
            throw new SriNoDisponibleException("El SRI no respondió. Intente más tarde o ingrese los datos manualmente.", ex);
        }

        // Encontrados: 12 h (el catastro cambia poco). No encontrados: 10 min, por si se registra luego.
        cache.Set(clave, resultado, resultado is null ? TimeSpan.FromMinutes(10) : TimeSpan.FromHours(12));
        return resultado;
    }

    private async Task<ResultadoSri?> ConsultarRucAsync(string ruc, CancellationToken ct)
    {
        var respuesta = await http.GetAsync($"ConsolidadoContribuyente/obtenerPorNumerosRuc?&ruc={ruc}", ct);
        if (respuesta.StatusCode == HttpStatusCode.NoContent) return null;
        respuesta.EnsureSuccessStatusCode();

        var datos = (await respuesta.Content.ReadFromJsonAsync<List<ContribuyenteDto>>(ct))?.FirstOrDefault();
        if (datos is null) return null;

        var alertas = new List<string>();
        if (!string.Equals(datos.EstadoContribuyenteRuc, "ACTIVO", StringComparison.OrdinalIgnoreCase))
            alertas.Add($"RUC en estado {datos.EstadoContribuyenteRuc}{Motivo(datos.MotivoCancelacionSuspension)}.");
        if (EsSi(datos.ContribuyenteFantasma))
            alertas.Add("El SRI lo cataloga como EMPRESA FANTASMA: no acepte sus facturas.");
        if (EsSi(datos.TransaccionesInexistente))
            alertas.Add("El SRI lo registra con TRANSACCIONES INEXISTENTES.");
        if (EsSi(datos.ContribuyenteEspecial)) alertas.Add("Contribuyente especial.");
        if (EsSi(datos.AgenteRetencion)) alertas.Add("Agente de retención.");

        return new ResultadoSri(ruc, TipoIdentificacion.Ruc, datos.RazonSocial?.Trim() ?? string.Empty,
            await DireccionMatrizAsync(ruc, ct), datos.EstadoContribuyenteRuc, datos.ActividadEconomicaPrincipal,
            datos.TipoContribuyente, datos.Regimen, alertas);
    }

    private async Task<string?> DireccionMatrizAsync(string ruc, CancellationToken ct)
    {
        try
        {
            var establecimientos = await http.GetFromJsonAsync<List<EstablecimientoDto>>(
                $"Establecimiento/consultarPorNumeroRuc?numeroRuc={ruc}", ct) ?? [];
            var abiertos = establecimientos.Where(e => EsIgual(e.Estado, "ABIERTO")).ToList();
            return (abiertos.FirstOrDefault(e => EsSi(e.Matriz)) ?? abiertos.OrderBy(e => e.NumeroEstablecimiento).FirstOrDefault())
                ?.DireccionCompleta;
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException)
        {
            return null; // La dirección es opcional: no se pierde la consulta principal por ella
        }
    }

    private async Task<ResultadoSri?> ConsultarCedulaAsync(string cedula, CancellationToken ct)
    {
        var respuesta = await http.GetAsync($"Persona/obtenerPorTipoIdentificacion?numeroIdentificacion={cedula}&tipoIdentificacion=C", ct);
        if (respuesta.StatusCode == HttpStatusCode.NoContent) return null;
        respuesta.EnsureSuccessStatusCode();

        var persona = await respuesta.Content.ReadFromJsonAsync<PersonaDto>(ct);
        if (string.IsNullOrWhiteSpace(persona?.NombreCompleto)) return null;
        return new ResultadoSri(cedula, TipoIdentificacion.Cedula, persona.NombreCompleto.Trim(), null, null, null, "PERSONA NATURAL", null, []);
    }

    private static string Motivo(System.Text.Json.JsonElement? motivo) =>
        motivo is { ValueKind: not System.Text.Json.JsonValueKind.Null } m ? $" ({m})" : "";

    private static bool EsSi(string? valor) => EsIgual(valor, "SI");
    private static bool EsIgual(string? a, string b) => string.Equals(a?.Trim(), b, StringComparison.OrdinalIgnoreCase);

    private record ContribuyenteDto(
        string? NumeroRuc, string? RazonSocial, string? EstadoContribuyenteRuc, string? ActividadEconomicaPrincipal,
        string? TipoContribuyente, string? Regimen, string? AgenteRetencion, string? ContribuyenteEspecial,
        string? ContribuyenteFantasma, string? TransaccionesInexistente, System.Text.Json.JsonElement? MotivoCancelacionSuspension);

    private record EstablecimientoDto(string? DireccionCompleta, string? Estado, string? NumeroEstablecimiento, string? Matriz);

    private record PersonaDto(string? Identificacion, string? NombreCompleto);
}
