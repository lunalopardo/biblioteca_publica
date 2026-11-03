// Lógica común de los formularios de libros: portada (vista previa y quitar) y búsquedas por AJAX de autor y editorial.

document.addEventListener('DOMContentLoaded', function () {

    // Portada
    const inputPortada = document.getElementById('PortadaFile');
    const imgPreview = document.getElementById('imgPreview');
    const placeholder = document.getElementById('portadaPlaceholder');
    const btnQuitar = document.getElementById('btnQuitarPortada');
    const campoEliminar = document.getElementById('EliminarPortada');

    // Al elegir un archivo se muestra la vista previa
    if (inputPortada) {
        inputPortada.addEventListener('change', function () {
            if (!inputPortada.files || !inputPortada.files[0]) return;

            const reader = new FileReader();
            reader.onload = function (e) {
                imgPreview.src = e.target.result;
                imgPreview.classList.remove('is-hidden');
                if (placeholder) placeholder.classList.add('is-hidden');

                // Si había pedido quitar la portada, ahora la va a reemplazar
                if (campoEliminar) campoEliminar.value = 'false';
                if (btnQuitar) btnQuitar.classList.remove('is-hidden');
            };
            reader.readAsDataURL(inputPortada.files[0]);
        });
    }

    // La portada se elimina cuando se aprieta "Guardar Cambios".
    if (btnQuitar && campoEliminar) {
        btnQuitar.addEventListener('click', function () {
            campoEliminar.value = 'true';
            if (inputPortada) inputPortada.value = '';

            imgPreview.classList.add('is-hidden');
            if (placeholder) {
                placeholder.textContent = 'La portada se quitará al guardar los cambios.';
                placeholder.classList.remove('is-hidden');
            }
            btnQuitar.classList.add('is-hidden');
        });
    }

    // Select2 con búsqueda en el servidor
    function activarBusqueda(selector, urlBusqueda, textoPlaceholder) {
        $(selector).select2({
            language: 'es',
            placeholder: textoPlaceholder,
            minimumInputLength: 1,
            ajax: {
                delay: 250,
                dataType: 'json',
                url: function (params) {
                    return `${urlBusqueda}?q=${encodeURIComponent(params.term || '')}`;
                },
                processResults: function (res) {
                    return { results: res.map(i => ({ id: i.id, text: i.text })) };
                }
            }
        });
    }

    activarBusqueda('#IdAutor', '/Autores/Buscar', 'Escriba para buscar autor...');
    activarBusqueda('#IdEditorial', '/Editoriales/Buscar', 'Escriba para buscar editorial...');

    // Los géneros son pocos: se cargan en la página y Select2 solo filtra
    $('#GenerosSeleccionados').select2({
        language: 'es',
        placeholder: 'Busque o seleccione los géneros...'
    });
});