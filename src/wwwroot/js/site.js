// Calendario (flatpickr) para todos los campos de fecha
function activarCalendario(input, opciones) {
    if (!window.flatpickr) return null;
    if (input._flatpickr) return input._flatpickr;

    return flatpickr(input, Object.assign({
        locale: 'es',
        dateFormat: 'Y-m-d',
        altInput: true,
        altFormat: 'd/m/Y',
        minDate: input.min || null,
        maxDate: input.max || null,
        disableMobile: true
    }, opciones));
}

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('input[type="date"]').forEach(input => activarCalendario(input));
});
