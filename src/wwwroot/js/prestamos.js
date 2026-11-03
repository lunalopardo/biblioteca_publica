const { createApp, ref, reactive, computed, onMounted } = Vue;

const root = document.getElementById('prestamos-app');
const token = root.dataset.token;
const diasPorDefecto = Number(root.dataset.dias);

async function request(url, options = {}) {
    const res = await fetch(url, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token,
            'X-Requested-With': 'XMLHttpRequest',
            ...options.headers
        }
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.message || 'Ocurrió un error inesperado.');
    return data;
}

// Fecha local en formato yyyy-mm-dd
const aISO = (d) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

createApp({
    setup() {
        // ref(...) = variable "reactiva": si cambia, Vue actualiza la pantalla solo
        const prestamos = ref([]);
        const cargando = ref(true);
        const buscar = ref('');
        const pagina = ref(1);
        const totalPaginas = ref(1);

        const mensaje = ref('');
        const mensajeEsError = ref(false);

        // Formulario de nuevo préstamo
        const mostrarModal = ref(false);
        const guardando = ref(false);
        const errorModal = ref('');
        const form = reactive({ idAfiliado: null, idEjemplar: null, fechaDevolucionPactada: '' });
        const busquedaAfiliado = ref('');
        const busquedaEjemplar = ref('');
        const afiliados = ref([]);
        const ejemplares = ref([]);

        const confirmacion = ref(null);

        // computed = valor que se recalcula solo
        const manana = computed(() => {
            const d = new Date();
            d.setDate(d.getDate() + 1);
            return aISO(d);
        });

        const mostrarMensaje = (texto, esError) => {
            mensaje.value = texto;
            mensajeEsError.value = esError;
        };

        const formatearFecha = (valor) =>
            valor ? valor.substring(0, 10).split('-').reverse().join('/') : '-';

        // LEER
        const cargarPrestamos = async () => {
            cargando.value = true;
            try {
                const data = await request(
                    `/Prestamos/Listar?buscar=${encodeURIComponent(buscar.value)}&pagina=${pagina.value}`);
                prestamos.value = data.items;
                totalPaginas.value = data.totalPaginas;
            } catch (err) {
                mostrarMensaje(err.message, true);
            } finally {
                cargando.value = false;
            }
        };

        const buscarPrestamos = () => { pagina.value = 1; cargarPrestamos(); };
        const limpiarBusqueda = () => { buscar.value = ''; buscarPrestamos(); };
        const irAPagina = (n) => {
            if (n < 1 || n > totalPaginas.value) return;
            pagina.value = n;
            cargarPrestamos();
        };

        // Búsquedas
        const buscarAfiliados = async () => {
            form.idAfiliado = null;
            try {
                afiliados.value = await request(
                    `/Afiliados/BuscarAsincrona?term=${encodeURIComponent(busquedaAfiliado.value)}`);
            } catch (err) {
                errorModal.value = err.message;
            }
        };

        const buscarEjemplares = async () => {
            form.idEjemplar = null;
            try {
                ejemplares.value = await request(
                    `/Prestamos/BuscarEjemplares?term=${encodeURIComponent(busquedaEjemplar.value)}`);
            } catch (err) {
                errorModal.value = err.message;
            }
        };

        // CREAR
        const abrirModalCrear = () => {
            const pactada = new Date();
            pactada.setDate(pactada.getDate() + diasPorDefecto);

            form.idAfiliado = null;
            form.idEjemplar = null;
            form.fechaDevolucionPactada = aISO(pactada);
            busquedaAfiliado.value = '';
            busquedaEjemplar.value = '';
            errorModal.value = '';
            mensaje.value = '';
            mostrarModal.value = true;

            buscarAfiliados();
            buscarEjemplares();
        };

        const cerrarModal = () => { mostrarModal.value = false; };

        const guardarPrestamo = async () => {
            if (!form.idAfiliado || !form.idEjemplar) {
                errorModal.value = 'Seleccioná un afiliado y un ejemplar de las listas.';
                return;
            }

            guardando.value = true;
            errorModal.value = '';
            try {
                const data = await request('/Prestamos/Crear', {
                    method: 'POST',
                    body: JSON.stringify({
                        idAfiliado: form.idAfiliado,
                        idEjemplar: form.idEjemplar,
                        fechaDevolucionPactada: form.fechaDevolucionPactada
                    })
                });
                cerrarModal();
                mostrarMensaje(data.message, false);
                await cargarPrestamos();
            } catch (err) {
                errorModal.value = err.message;
            } finally {
                guardando.value = false;
            }
        };

        // ACTUALIZAR Y ELIMINAR
        const pedirConfirmacion = (tipo, prestamo) => {
            mensaje.value = '';
            confirmacion.value = { tipo, prestamo };
        };

        const confirmar = async () => {
            const { tipo, prestamo } = confirmacion.value;
            const url = tipo === 'devolver'
                ? `/Prestamos/Devolver?id=${prestamo.id}`
                : `/Prestamos/Anular?id=${prestamo.id}`;

            try {
                const data = await request(url, { method: 'POST' });
                mostrarMensaje(data.message, false);
            } catch (err) {
                mostrarMensaje(err.message, true);
            }
            confirmacion.value = null;
            await cargarPrestamos();
        };

        // Se ejecuta una vez cuando Vue termina de montar la página
        onMounted(cargarPrestamos);

        return {
            prestamos, cargando, buscar, pagina, totalPaginas,
            mensaje, mensajeEsError,
            mostrarModal, guardando, errorModal, form,
            busquedaAfiliado, busquedaEjemplar, afiliados, ejemplares,
            confirmacion, manana,
            formatearFecha, buscarPrestamos, limpiarBusqueda, irAPagina,
            buscarAfiliados, buscarEjemplares,
            abrirModalCrear, cerrarModal, guardarPrestamo,
            pedirConfirmacion, confirmar
        };
    }
})
    // v-calendario: el input de fecha está dentro del modal (v-if), así que el calendario se activa cuando Vue lo crea
    .directive('calendario', {
        mounted(el) {
            activarCalendario(el, {
                onChange: (_, valor) => {
                    el.value = valor;
                    el.dispatchEvent(new Event('input'));
                }
            });
        },
        unmounted(el) {
            if (el._flatpickr) el._flatpickr.destroy();
        }
    })
    .mount(root);