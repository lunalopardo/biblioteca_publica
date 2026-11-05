// Lógica común de los formularios de libros: búsquedas por AJAX de autor y editorial.

document.addEventListener('DOMContentLoaded', function () {

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