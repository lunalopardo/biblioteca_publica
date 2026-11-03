// Cierra solos los mensajes de éxito (los de error se quedan hasta que cambie la página).
document.addEventListener('DOMContentLoaded', function () {
    const DURACION_MS = 4000;

    document.querySelectorAll('#alerta-mensaje, .alert-success').forEach(function (alerta) {
        setTimeout(function () {
            alerta.style.transition = 'opacity 0.4s ease';
            alerta.style.opacity = '0';
            setTimeout(function () { alerta.remove(); }, 400);
        }, DURACION_MS);
    });
});