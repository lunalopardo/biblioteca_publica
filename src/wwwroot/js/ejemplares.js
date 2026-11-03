const { createApp, ref, reactive, onMounted } = Vue;

const root = document.getElementById('ejemplares-app');
const idLibro = Number(root.dataset.idLibro);
const token = root.dataset.token;

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

createApp({
    setup() {
        const ejemplares = ref([]);
        const cargando = ref(true);
        const guardando = ref(false);
        const mostrarModal = ref(false);
        const editandoId = ref(null);
        const estadoActual = ref('');
        const error = ref('');
        const form = reactive({ codigo: null, ubicacion: '' });

        const cargarEjemplares = async () => {
            cargando.value = true;
            try {
                ejemplares.value = await request(`/Ejemplares/ObtenerPorLibro?idLibro=${idLibro}`);
            } catch (err) {
                console.error('Error al cargar ejemplares', err);
                error.value = err.message;
            } finally {
                cargando.value = false;
            }
        };

        const abrirModal = (ejemplar = null) => {
            editandoId.value = ejemplar?.id ?? null;
            estadoActual.value = ejemplar?.estado ?? '';
            form.codigo = ejemplar?.codigoInventario ?? null;
            form.ubicacion = ejemplar?.ubicacion ?? '';
            error.value = '';
            mostrarModal.value = true;
        };
        const abrirModalCrear = () => abrirModal();
        const abrirModalEditar = (ejemplar) => abrirModal(ejemplar);
        const cerrarModal = () => { mostrarModal.value = false; };

        const guardarEjemplar = async () => {
            guardando.value = true;
            error.value = '';
            const url = editandoId.value ? '/Ejemplares/EditarAjax' : '/Ejemplares/CrearAjax';
            try {
                await request(url, {
                    method: 'POST',
                    body: JSON.stringify({
                        id: editandoId.value ?? 0,
                        idLibro,
                        codigoInventario: form.codigo,
                        ubicacion: form.ubicacion?.trim() || null
                    })
                });
                cerrarModal();
                await cargarEjemplares();
            } catch (err) {
                error.value = err.message;
            } finally {
                guardando.value = false;
            }
        };

        const eliminarEjemplar = async (id) => {
            if (!confirm('¿Está seguro de eliminar este ejemplar?')) return;
            try {
                await request(`/Ejemplares/EliminarAjax?id=${id}`, { method: 'POST' });
                await cargarEjemplares();
            } catch (err) {
                alert(err.message);
            }
        };

        onMounted(cargarEjemplares);

        return {
            ejemplares, cargando, guardando, mostrarModal, editandoId, estadoActual, error, form,
            abrirModalCrear, abrirModalEditar, cerrarModal, guardarEjemplar, eliminarEjemplar
        };
    }
}).mount(root);