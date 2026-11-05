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

// Selector de imagen de los formularios (Views/Shared/_SelectorImagen.cshtml)
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-selector-imagen]').forEach(selector => {
        const input = selector.querySelector('input[type="file"]');
        if (!input) return;

        const vistaPrevia = selector.querySelector('[data-vista-previa]');
        const sinImagen = selector.querySelector('[data-sin-imagen]');
        const botonElegir = selector.querySelector('[data-boton-elegir]');
        const botonQuitar = selector.querySelector('[data-boton-quitar]');
        const eliminar = selector.querySelector('[data-eliminar]');

        const mostrar = (src) => {
            vistaPrevia.classList.toggle('is-hidden', !src);
            sinImagen.classList.toggle('is-hidden', !!src);
            botonQuitar.classList.toggle('is-hidden', !src);
            botonElegir.textContent = src ? selector.dataset.textoCambiar : selector.dataset.textoElegir;

            if (src) vistaPrevia.src = src;
            else vistaPrevia.removeAttribute('src');
        };

        input.addEventListener('change', function () {
            if (!input.files || !input.files[0]) return;

            const reader = new FileReader();
            reader.onload = (e) => mostrar(e.target.result);
            reader.readAsDataURL(input.files[0]);
            eliminar.value = 'false';
        });

        // La imagen se borra recién al guardar el formulario
        botonQuitar.addEventListener('click', function () {
            input.value = '';
            eliminar.value = 'true';
            mostrar(null);
        });
    });
});

// Devuelve una promesa: true si aceptó, false si canceló o cerró el modal.
function confirmar(mensaje, { titulo = 'Confirmar acción', textoBoton = 'Confirmar', tipo = 'danger' } = {}) {
    const elemento = document.getElementById('modalConfirmacion');
    if (!elemento || !window.bootstrap) return Promise.resolve(window.confirm(mensaje));

    const tituloModal = elemento.querySelector('#modalConfirmacionTitulo');
    const boton = elemento.querySelector('#modalConfirmacionAceptar');

    tituloModal.textContent = titulo;
    tituloModal.classList.toggle('text-danger', tipo === 'danger');
    elemento.querySelector('#modalConfirmacionTexto').textContent = mensaje;
    boton.textContent = textoBoton;
    boton.className = `btn btn-${tipo}`;

    const modal = bootstrap.Modal.getOrCreateInstance(elemento);

    return new Promise(resolve => {
        let aceptado = false;
        const alAceptar = () => {
            aceptado = true;
            modal.hide();
        };

        boton.addEventListener('click', alAceptar, { once: true });
        elemento.addEventListener('hidden.bs.modal', () => {
            boton.removeEventListener('click', alAceptar);
            resolve(aceptado);
        }, { once: true });

        modal.show();
    });
}

// Formularios con data-confirmar="mensaje": se envían solo si se acepta el modal
document.addEventListener('submit', async function (event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.dataset.confirmar) return;

    event.preventDefault();

    const aceptado = await confirmar(form.dataset.confirmar, {
        titulo: form.dataset.confirmarTitulo,
        textoBoton: form.dataset.confirmarBoton,
        tipo: form.dataset.confirmarTipo
    });

    if (aceptado) form.submit();
});
