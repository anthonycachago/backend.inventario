// Modo claro / oscuro con Bootstrap 5.3 (atributo data-bs-theme en <html>).
// Preferencia guardada en localStorage; si no hay, se sigue la del sistema operativo.
(function () {
    const CLAVE = 'inventario.tema';
    const consulta = window.matchMedia('(prefers-color-scheme: dark)');

    function leer() {
        try { return localStorage.getItem(CLAVE); } catch { return null; }
    }

    function guardar(tema) {
        try { localStorage.setItem(CLAVE, tema); } catch { /* navegación privada */ }
    }

    function preferido() {
        return leer() ?? (consulta.matches ? 'dark' : 'light');
    }

    function aplicar(tema) {
        document.documentElement.setAttribute('data-bs-theme', tema);
    }

    function actual() {
        return document.documentElement.getAttribute('data-bs-theme') ?? 'light';
    }

    aplicar(preferido());

    consulta.addEventListener('change', () => { if (!leer()) aplicar(preferido()); });

    // La navegación mejorada de Blazor reemplaza el <html> con el del servidor: volver a aplicar.
    document.addEventListener('DOMContentLoaded', () => {
        window.Blazor?.addEventListener?.('enhancedload', () => aplicar(preferido()));
    });

    window.temaInventario = {
        actual: actual,
        alternar: function () {
            const nuevo = actual() === 'dark' ? 'light' : 'dark';
            aplicar(nuevo);
            guardar(nuevo);
            return nuevo;
        }
    };
})();
