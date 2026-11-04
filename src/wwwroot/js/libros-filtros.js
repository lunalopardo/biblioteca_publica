document.addEventListener('DOMContentLoaded', function () {
    $('#filtroAutor').select2({
        language: 'es',
        placeholder: 'Todos los autores',
        allowClear: true,
        minimumInputLength: 1,
        ajax: {
            delay: 250,
            dataType: 'json',
            url: function (params) {
                return `/Autores/Buscar?q=${encodeURIComponent(params.term || '')}`;
            },
            processResults: function (res) {
                return { results: res.map(i => ({ id: i.id, text: i.text })) };
            }
        }
    });
});
