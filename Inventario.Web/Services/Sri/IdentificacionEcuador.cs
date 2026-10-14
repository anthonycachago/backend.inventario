using Inventario.Web.Entity;

namespace Inventario.Web.Services.Sri;

/// <summary>Validación local de cédula y RUC ecuatorianos (sin red).</summary>
public static class IdentificacionEcuador
{
    public const string ConsumidorFinal = "9999999999999";

    /// <summary>Devuelve null si es válida, o el motivo por el que no lo es.</summary>
    public static string? Validar(TipoIdentificacion tipo, string? valor)
    {
        valor = valor?.Trim() ?? string.Empty;
        return tipo switch
        {
            TipoIdentificacion.Cedula => ValidarCedula(valor),
            TipoIdentificacion.Ruc => ValidarRuc(valor),
            TipoIdentificacion.ConsumidorFinal => valor == ConsumidorFinal ? null : $"El consumidor final se identifica como {ConsumidorFinal}.",
            TipoIdentificacion.Pasaporte => valor.Length is >= 5 and <= 20 ? null : "El pasaporte debe tener entre 5 y 20 caracteres.",
            _ => "Tipo de identificación no soportado."
        };
    }

    public static string? ValidarCedulaORuc(string? valor) =>
        Deducir(valor) is TipoIdentificacion.Cedula or TipoIdentificacion.Ruc
            ? Validar(Deducir(valor)!.Value, valor)
            : "Ingrese una cédula (10 dígitos) o un RUC (13 dígitos).";

    /// <summary>Deduce el tipo por la longitud: 10 dígitos = cédula, 13 = RUC.</summary>
    public static TipoIdentificacion? Deducir(string? valor) => valor?.Trim() switch
    {
        ConsumidorFinal => TipoIdentificacion.ConsumidorFinal,
        { Length: 10 } v when v.All(char.IsAsciiDigit) => TipoIdentificacion.Cedula,
        { Length: 13 } v when v.All(char.IsAsciiDigit) => TipoIdentificacion.Ruc,
        _ => null
    };

    public static string? ValidarCedula(string c)
    {
        if (c.Length != 10 || !c.All(char.IsAsciiDigit)) return "La cédula debe tener 10 dígitos.";
        if (!ProvinciaValida(c)) return "Los dos primeros dígitos de la cédula no corresponden a una provincia.";
        if (c[2] - '0' >= 6) return "El tercer dígito de una cédula debe ser menor a 6.";

        // Módulo 10: coeficientes 2,1,2,1... restando 9 a los productos mayores a 9
        var suma = 0;
        for (var i = 0; i < 9; i++)
        {
            var p = (c[i] - '0') * (i % 2 == 0 ? 2 : 1);
            suma += p > 9 ? p - 9 : p;
        }
        var verificador = (10 - suma % 10) % 10;
        return verificador == c[9] - '0' ? null : "El dígito verificador de la cédula no es válido.";
    }

    public static string? ValidarRuc(string r)
    {
        if (r.Length != 13 || !r.All(char.IsAsciiDigit)) return "El RUC debe tener 13 dígitos.";
        if (!ProvinciaValida(r)) return "Los dos primeros dígitos del RUC no corresponden a una provincia.";

        var tercero = r[2] - '0';
        switch (tercero)
        {
            case < 6: // Persona natural: cédula + establecimiento
                if (ValidarCedula(r[..10]) is not null) return "Los 10 primeros dígitos del RUC no son una cédula válida.";
                return r[10..] == "000" ? "El establecimiento del RUC no puede ser 000." : null;
            case 6: // Entidad pública: el establecimiento usa 4 dígitos
                return r[9..] == "0000" ? "El establecimiento del RUC no puede ser 0000." : null;
            case 9: // Sociedad privada
                // No se exige el dígito verificador módulo 11: el SRI emite RUC de sociedades que ya no lo cumplen.
                return r[10..] == "000" ? "El establecimiento del RUC no puede ser 000." : null;
            default:
                return "El tercer dígito del RUC no es válido (0-5 natural, 6 pública, 9 sociedad).";
        }
    }

    // 01-24 provincias, 30 ecuatorianos registrados en el exterior
    private static bool ProvinciaValida(string v)
    {
        var provincia = int.Parse(v[..2]);
        return provincia is >= 1 and <= 24 or 30;
    }
}
