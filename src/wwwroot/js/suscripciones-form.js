// Formulario de suscripción: búsqueda del afiliado con Select2 y AJAX en el servidor,
// y calendarios que no dejan elegir días que ya cubre otra suscripción del mismo afiliado.

document.addEventListener('DOMContentLoaded', function () {
    const selectAfiliado = $('#IdAfiliado');
    const ayuda = document.getElementById('ayudaFechas');

    // Los calendarios los crea site.js para todos los input[type=date]
    const fpInicio = document.getElementById('FechaInicio')._flatpickr;
    const fpFin = document.getElementById('FechaFin')._flatpickr;

    let ocupadas = [];

    const sumarDias = (fecha, dias) => {
        const r = new Date(fecha);
        r.setDate(r.getDate() + dias);
        return r;
    };

    const formatear = (fecha) => flatpickr.formatDate(fecha, 'd/m/Y');

    const primerDiaLibre = (fecha) => {
        let resultado = fecha;
        let seMovio = true;
        while (seMovio) {
            seMovio = false;
            for (const r of ocupadas) {
                if (resultado >= r.desde && resultado <= r.hasta) {
                    resultado = sumarDias(r.hasta, 1);
                    seMovio = true;
                }
            }
        }
        return resultado;
    };

    const ajustarFin = () => {
        const inicio = fpInicio.selectedDates[0];
        if (!inicio) return;

        const siguiente = ocupadas
            .filter(r => r.desde > inicio)
            .sort((a, b) => a.desde - b.desde)[0];
        const limite = siguiente ? sumarDias(siguiente.desde, -1) : null;

        fpFin.set('minDate', sumarDias(inicio, 1));
        fpFin.set('maxDate', limite);

        const fin = fpFin.selectedDates[0];
        if (fin && fin > inicio && (!limite || fin <= limite)) return;

        let propuesta = new Date(inicio);
        propuesta.setMonth(propuesta.getMonth() + 1);
        if (limite && propuesta > limite) propuesta = limite;

        if (propuesta > inicio) {
            fpFin.setDate(propuesta, false);
        } else {
            fpFin.clear();
        }
    };

    const mostrarAyuda = () => {
        const hoy = new Date();
        hoy.setHours(0, 0, 0, 0);
        const ultima = ocupadas
            .filter(r => r.hasta >= hoy)
            .sort((a, b) => b.hasta - a.hasta)[0];

        if (ultima) {
            ayuda.textContent = `Este afiliado ya tiene suscripción hasta el ${formatear(ultima.hasta)}. ` +
                'Los días ocupados aparecen deshabilitados en el calendario.';
            ayuda.classList.remove('is-hidden');
        } else {
            ayuda.classList.add('is-hidden');
        }
    };

    const cargarOcupadas = async (idAfiliado) => {
        ocupadas = [];
        if (idAfiliado) {
            try {
                const res = await fetch(`/Suscripciones/Ocupadas?idAfiliado=${encodeURIComponent(idAfiliado)}`);
                const datos = res.ok ? await res.json() : [];
                ocupadas = datos.map(r => ({
                    desde: flatpickr.parseDate(r.desde, 'Y-m-d'),
                    hasta: flatpickr.parseDate(r.hasta, 'Y-m-d')
                }));
            } catch {
                
            }
        }

        const deshabilitadas = ocupadas.map(r => ({ from: r.desde, to: r.hasta }));
        fpInicio.set('disable', deshabilitadas);
        fpFin.set('disable', deshabilitadas);

        const inicio = fpInicio.selectedDates[0];
        if (inicio) {
            const libre = primerDiaLibre(inicio);
            if (libre.getTime() !== inicio.getTime()) fpInicio.setDate(libre, false);
        }

        ajustarFin();
        mostrarAyuda();
    };

    selectAfiliado.select2({
        language: 'es',
        placeholder: '-- Buscar afiliado por nombre, apellido o email --',
        minimumInputLength: 1,
        width: '100%',
        ajax: {
            delay: 250,
            dataType: 'json',
            url: function (params) {
                return `/Afiliados/BuscarAsincrona?term=${encodeURIComponent(params.term || '')}`;
            },
            processResults: function (res) {
                return { results: res.map(i => ({ id: i.id, text: i.text })) };
            }
        }
    });

    selectAfiliado.on('change', () => cargarOcupadas(selectAfiliado.val()));
    fpInicio.config.onChange.push(ajustarFin);

    // Si la vista ya viene con un afiliado (desde su historial o tras un error de validación)
    cargarOcupadas(selectAfiliado.val());
});
