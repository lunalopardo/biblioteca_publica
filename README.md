# 📚 Biblioteca Pública – Sistema de Gestión

Proyecto final de **Laboratorio II** (2026) · Estudiante: **Luna Morena Lopardo**

Sistema web MVC para que los empleados de una biblioteca pública gestionen el catálogo de libros, sus ejemplares, los afiliados, las cuotas de afiliación, los préstamos y las multas por devolución tardía.

---

## 1. Descripción del proyecto

### El problema

Una biblioteca pública necesita saber en todo momento:

- Qué libros tiene y cuántas copias (ejemplares) hay de cada uno.
- Qué ejemplares están disponibles y cuáles prestados.
- Quiénes son sus afiliados y si tienen la cuota al día.
- Quién se llevó cada libro, hasta cuándo, y si lo devolvió a tiempo.
- Qué empleado cargó o anuló cada operación (auditoría).

### La solución

Una aplicación web interna, usada solo por empleados y administradores, que centraliza todo eso y aplica automáticamente las reglas de negocio (por ejemplo, que no se pueda prestar un libro a un afiliado con la cuota vencida).

### Roles

| Rol | Qué puede hacer |
|---|---|
| **Administrador** | Todo lo del empleado, más la gestión de usuarios (ver, crear, editar, cambiar el rol, dar de baja y reactivar) y la **auditoría**. Es el único que puede dar de baja el **catálogo**: libros, ejemplares, autores, editoriales, géneros y nacionalidades. |
| **Empleado** | Creación, lectura y edición de todas las entidades. Puede dar de baja lo relacionado con la gestión de afiliados (afiliados, suscripciones y pagos) y anular préstamos. Solo puede ver y editar su propio perfil. |

---

## 2. Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core MVC (C#) |
| Acceso a datos | ADO.NET con **MySqlConnector** (consultas SQL propias, sin ORM) |
| Base de datos | MySQL |
| Frontend | Razor Views, Bootstrap, CSS propio (`site.css`, `formularios.css`) |
| SPA / AJAX | **Vue 3** (por CDN) + `fetch` con JSON |
| Íconos | Material Symbols |
| Seguridad | Autenticación por cookies, roles, tokens antiforgery, contraseñas encriptadas |
| API | Web API REST con **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`) |

---

## 3. Modelo de datos

![`/docs/der_final.png`](docs/der_final.png)


### Tablas (12)

| Tabla | Descripción |
|---|---|
| `usuario` | Empleados y administradores. Incluye rol y avatar. |
| `afiliado` | Socios de la biblioteca. |
| `suscripcion` | Período durante el cual un afiliado puede usar la biblioteca (desde, hasta, monto). |
| `pago` | Cuotas y multas. Tipo `Cuota` (se asocia a una suscripción) o `Multa` (se asocia a un préstamo). |
| `libro` | Catálogo. Con portada, autor y editorial. |
| `ejemplar` | Cada copia física de un libro. Estado `Disponible` / `Prestado`. |
| `prestamo` | Préstamo de un ejemplar a un afiliado. |
| `autor` | Autores, con foto y biografía. |
| `nacionalidad` | Catálogo de nacionalidades. |
| `editorial` | Catálogo de editoriales. |
| `genero` | Catálogo de géneros. |
| `libro_genero` | Tabla intermedia de la relación **N:M** entre libros y géneros. |

### Relaciones destacadas

- **1:N:** Libro → Ejemplar, Autor → Libro, Afiliado → Préstamo, Afiliado → Suscripción, entre otras.
- **N:M:** Libro ↔ Género mediante `libro_genero`.
- **Entidad que depende de otras dos:** `Prestamo` depende de `Afiliado` y de `Ejemplar`.
- **Auditoría:** `prestamo` y `pago` guardan quién y cuándo los creó y anuló (`creado_por_user_id`, `creado_en`, `anulado_por_user_id`, `anulado_en`). `prestamo` también guarda quién y cuándo registró la devolución (`devuelto_por_user_id`, `devuelto_en`).
- **Borrado lógico:** todas las entidades usan la bandera `activo` en lugar de `DELETE`, para no romper el historial.

---

## 4. Reglas de negocio implementadas

### Suscripciones y pagos

- Un afiliado se considera **activo** si tiene una suscripción **vigente hoy** (entre `fecha_inicio` y `fecha_fin`) **y pagada**. El estado se **calcula**, no se guarda.
- No se pueden crear suscripciones que se superpongan en fechas para el mismo afiliado.
- El monto de la cuota sale de la suscripción en el servidor, no del formulario.
- Una suscripción **con pago registrado no se puede anular**. Primero hay que anular el pago.
- Una suscripción anulada por error (sin pago) se da de baja de forma lógica.
- Anular un pago deja registrado qué usuario lo hizo.

### Préstamos

Al crear un préstamo, el servidor comprueba:

1. El afiliado **no está sancionado**. La sanción no se carga a mano: un afiliado está sancionado mientras tenga **préstamos vencidos sin devolver** (se calcula en SQL, igual que el estado de la suscripción).
2. Tiene una **suscripción vigente y pagada**.
3. El ejemplar está **disponible**.

Desde el listado de afiliados, el botón **Préstamos** muestra solo los de ese afiliado, con los vencidos primero (se resalta cuando está sancionado).

Si todo se cumple, el préstamo y el cambio de estado del ejemplar a `Prestado` se guardan **en una transacción**: o se hacen las dos cosas, o ninguna.

### Devoluciones y multas

- Al devolver, se registra la fecha real y el ejemplar vuelve a `Disponible`.
- Si la devolución es posterior a la fecha pactada, se genera **automáticamente un pago de tipo `Multa`** asociado al préstamo ($100 por día de atraso, configurable en `ServicioPrestamo`).
- Un préstamo cargado por error se puede **anular** mientras no esté devuelto; el ejemplar queda disponible otra vez.

### Auditoría

- La vista **Auditoría** (solo administrador, de solo lectura) muestra quién registró, devolvió o anuló cada préstamo y pago, con fecha y hora. Se puede filtrar por usuario, operación y fechas, con paginado en SQL.

---

## 5. Evidencias de requerimientos mínimos


| # | Requerimiento mínimo | Estado | Enlace al código | Evidencia |
|---|---|---|---|---|
| 1 | **6+ tablas y relaciones 1:N y N:M** (12 tablas; N:M Libro–Género; Préstamo depende de Afiliado y Ejemplar) | ✅ Cumplido | [`Models/`](src/Models) · [`Repositories/`](src/Repositories) · [`database/biblioteca_db.sql`](database/biblioteca_db.sql) | DER en [`docs/der_final.png`](docs/der_final.png) |
| 2 | **Login, roles y avatar** (cookies + claims; todo el sitio exige sesión; policy `Administrador` con `[Authorize(Policy = UsuarioActual.PolicyAdministrador)]`; avatar en perfil y alta de usuario) | ✅ Cumplido | [`Program.cs` (autenticación)](src/Program.cs#L17) · [`Program.cs` (sesión obligatoria y policy)](src/Program.cs#L68) · [`UsuariosController.Login`](src/Controllers/UsuariosController.cs#L55) · [`UsuariosController.ContraseniaValida`](src/Controllers/UsuariosController.cs#L321) · [`UsuariosController.IniciarSesionAsync`](src/Controllers/UsuariosController.cs#L328) · [`UsuarioActual.CrearClaims` (claims)](src/Helpers/UsuarioActual.cs#L19) · [`UsuariosController.Perfil` (avatar)](src/Controllers/UsuariosController.cs#L123) · [`RepositorioUsuario.ObtenerActivoPorLogin`](src/Repositories/RepositorioUsuario.cs#L11) · [`Helpers/UsuarioActual.cs`](src/Helpers/UsuarioActual.cs) · [`LibrosController.Delete` (solo admin)](src/Controllers/LibrosController.cs#L171) · [`Views/Usuarios/Login.cshtml`](src/Views/Usuarios/Login.cshtml) | Captura de pantalla de perfil con avatar en [`docs/evidencia_avatar.png`](docs/evidencia_avatar.png) |
| 3 | **Manejo de archivos** (portadas de libros, fotos de autores, avatares; extensión y tamaño validados) | ✅ Cumplido | [`Archivos.GuardarImagenAsync`](src/Helpers/Archivos.cs#L28) · [`Archivos.ValidarImagen`](src/Helpers/Archivos.cs#L11) · [`ImagenValidaAttribute`](src/Helpers/ImagenValidaAttribute.cs#L6) · [`LibrosController.Create` (portada)](src/Controllers/LibrosController.cs#L86) · [`AutoresController.Create` (foto)](src/Controllers/AutoresController.cs#L59) · [`UsuariosController.Create` (avatar)](src/Controllers/UsuariosController.cs#L303) · [`Views/Libros/Create.cshtml`](src/Views/Libros/Create.cshtml) | GIF mostrando subida en [`docs/evidencia_archivos.gif`](docs/evidencia_archivos.gif) |
| 4 | **ABM con Vue + AJAX** (Préstamos: alta, listado, devolución y anulación sin recargar) | ✅ Cumplido | [`Views/Prestamos/Index.cshtml`](src/Views/Prestamos/Index.cshtml) · [`wwwroot/js/prestamos.js`](src/wwwroot/js/prestamos.js) · [`PrestamosController.cs`](src/Controllers/PrestamosController.cs) | GIF del ABM dinámico en [`docs/evidencia_vue.gif`](docs/evidencia_vue.gif) |
| 5 | **Paginado por servidor** (`LIMIT/OFFSET` en SQL; se pide solo la página actual) | ✅ Cumplido | [`RepositorioPrestamo.ObtenerPaginado`](src/Repositories/RepositorioPrestamo.cs#L32) · [`RepositorioPago.ObtenerPaginado`](src/Repositories/RepositorioPago.cs#L11) · [`RepositorioAfiliado.ObtenerPaginado`](src/Repositories/RepositorioAfiliado.cs#L19) · [`RepositorioLibro.ObtenerPaginado`](src/Repositories/RepositorioLibro.cs#L25) · [`Paginacion.CargarPaginado`](src/Helpers/Paginacion.cs#L8) · [`Views/Shared/_Paginacion.cshtml`](src/Views/Shared/_Paginacion.cshtml) | Capturas del inspector de red enviando pagina=1 y pagina=2 en [`docs/evidencia_paginado1.png`](docs/evidencia_paginado1.png) y [`docs/evidencia_paginado2.png`](docs/evidencia_paginado2.png)|
| 6 | **Selección asíncrona de entidades relacionadas** (afiliado y ejemplar al crear un préstamo; afiliado en la nueva suscripción; autor y editorial del libro con Select2; todo filtrado y limitado en SQL) | ✅ Cumplido | [`AfiliadosController.BuscarAsincrona`](src/Controllers/AfiliadosController.cs#L114) · [`RepositorioAfiliado.ObtenerParaSelect`](src/Repositories/RepositorioAfiliado.cs#L159) · [`PrestamosController.BuscarEjemplares`](src/Controllers/PrestamosController.cs#L69) · [`AutoresController.Buscar`](src/Controllers/AutoresController.cs#L145) · [`EditorialesController.Buscar`](src/Controllers/EditorialesController.cs#L109) · [`wwwroot/js/prestamos.js`](src/wwwroot/js/prestamos.js) · [`wwwroot/js/suscripciones-form.js`](src/wwwroot/js/suscripciones-form.js) · [`wwwroot/js/libros-form.js`](src/wwwroot/js/libros-form.js) | GIF buscando entidades en desplegable en [`docs/evidencia_busquedaajax.gif`](docs/evidencia_busquedaajax.gif) |
| 7 | **Web API REST con JWT** (login que emite el token; libros, afiliados y préstamos con `Authorization: Bearer`; 401 sin token o vencido, 403 por rol; mismas reglas que la web vía `ServicioPrestamo`) | ✅ Cumplido | [`Program.cs` (AddJwtBearer)](src/Program.cs#L35) · [`ServicioToken.Generar`](src/Services/ServicioToken.cs#L32) · [`AuthApiController.Login`](src/Controllers/Api/AuthApiController.cs#L32) · [`LibrosApiController`](src/Controllers/Api/LibrosApiController.cs) · [`AfiliadosApiController`](src/Controllers/Api/AfiliadosApiController.cs) · [`PrestamosApiController`](src/Controllers/Api/PrestamosApiController.cs) · [`ServicioPrestamo`](src/Services/ServicioPrestamo.cs) | Colección de pruebas en [`docs/postman/BibliotecaPublica.postman_collection.json`](docs/postman/BibliotecaPublica.postman_collection.json) · Captura del pedido con Bearer Token en [`docs/evidencia_api.png`](docs/evidencia_api.png) · GIF de la colección completa en [`docs/evidencia_api.gif`](docs/evidencia_api.gif) |

---

## 6. Cómo ejecutarlo en local

### Pasos

1. Clonar el repositorio.
2. Crear la base de datos e importar el script:
   ```sql
   CREATE DATABASE biblioteca_db CHARACTER SET utf8mb4;
   ```
   Luego importar `database/biblioteca_db.sql` (desde phpMyAdmin o por consola).
3. Configurar la cadena de conexión en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=biblioteca_db;User=root;Password=;"
   }
   ```
4. Ejecutar:
   ```bash
   cd src
   dotnet run
   ```
5. Abrir la dirección que indique la consola.

### Usuarios de prueba

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | admin | admin |
| Empleado | empleado | empleado |



### Imágenes de prueba

Las imágenes subidas (portadas, fotos de autores y avatares) se guardan en `src/wwwroot/uploads/`, y la base guarda solo la ruta (por ejemplo `/uploads/libros/<archivo>.jpg`).

Esa carpeta **se incluye a propósito en el repositorio**: los datos de prueba de la base apuntan a esos archivos, así que sin ellos las imágenes aparecerían rotas. Al clonar el repositorio e importar la base, las imágenes se ven sin hacer nada más.

### Probar la Web API

La aplicación web no consume la API (sus pantallas usan la cookie de sesión), así que se prueba con la colección [`docs/postman/BibliotecaPublica.postman_collection.json`](docs/postman/BibliotecaPublica.postman_collection.json) (formato Postman v2.1).

1. Con la aplicación corriendo, importar la colección. La variable `baseUrl` apunta a `http://localhost:5288` (modificar si se corre en otro puerto).
2. Ejecutar **Autenticación → Login administrador**: devuelve el token y lo guarda en la variable `token`.
3. El resto de los pedidos mandan `Authorization: Bearer {{token}}`. La carpeta **Seguridad** muestra los casos 401 (sin token o token inválido), 403 (empleado dando de baja un libro) y 404.

> ! **Registrar préstamo** solo da 201 si el afiliado tiene la suscripción paga y sin sanción, y el ejemplar está disponible. Antes de ejecutarlo, conviene revisar las variables `idAfiliado` e `idEjemplar` de la colección.

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Devuelve el JWT (`{ "email": "admin", "password": "admin" }`) |
| GET | `/api/libros?buscar=&pagina=1` | Catálogo paginado con ejemplares disponibles |
| GET | `/api/libros/{id}` | Detalle de un libro |
| DELETE | `/api/libros/{id}` | Baja de un libro (solo administrador) |
| GET | `/api/afiliados?buscar=&pagina=1` | Afiliados paginados |
| GET | `/api/afiliados/{id}` | Detalle de un afiliado |
| GET | `/api/prestamos?buscar=&idAfiliado=&pagina=1` | Préstamos paginados, opcionalmente de un afiliado |
| GET | `/api/prestamos/{id}` | Detalle de un préstamo |
| POST | `/api/prestamos` | Registrar préstamo (`idAfiliado`, `idEjemplar`, `fechaDevolucionPactada`) |
| POST | `/api/prestamos/{id}/devolucion` | Registrar devolución (genera la multa si hay atraso) |
| DELETE | `/api/prestamos/{id}` | Anular un préstamo no devuelto |

El token dura 60 minutos; la clave, el emisor y la duración están en la sección `Jwt` de `appsettings.json`.


---

## 7. Estado de avance y pendientes

### Hecho

**Catálogo**
- Libros, ejemplares, autores, editoriales, géneros y nacionalidades.
- Importación de libros desde **Open Library** (con idioma, descripción y portada).
- Autor y editorial del libro se eligen con **Select2**, buscando en el servidor (filtrado y `LIMIT` en SQL).
- Ubicación de cada ejemplar; su estado lo controlan los préstamos y no se puede dar de baja si está prestado.

**Afiliados, suscripciones y pagos**
- Reglas de suscripción y pago (sin superposición de fechas, monto tomado del servidor, anulación con auditoría).
- Listado de **suscripciones activas** (vigentes y programadas) con búsqueda y paginación.
- Alta de suscripción con Select2 para el afiliado y calendario que **deshabilita los días ya cubiertos** por otra suscripción.
- La **sanción se calcula** desde los préstamos vencidos sin devolver.
- Botón **Préstamos** en el listado de afiliados (resaltado si está sancionado), con los vencidos primero.

**Préstamos**
- Préstamos y devoluciones con Vue, transacciones y multa automática.
- Vista de **auditoría** para el administrador: quién registró, devolvió o anuló cada préstamo y pago, con fecha, hora y filtros.

**Usuarios, roles y seguridad**
- El empleado ve todo menos Usuarios y solo edita su propio perfil; el administrador gestiona usuarios (crear, editar, cambiar el rol, dar de baja y reactivar).
- Solo el administrador da de baja el catálogo (libros, ejemplares, autores, editoriales, géneros y nacionalidades); el empleado puede dar de baja lo relacionado con afiliados.
- Todo el sitio exige sesión (política global); un link sin sesión lleva al login y después vuelve a la página pedida.
- Policy `Administrador` basada en `EnRoles`.
- Contraseñas hasheadas en el controlador; tokens antiforgery en todos los formularios y llamadas AJAX.
- Las llamadas AJAX sin sesión o sin permiso reciben 401/403 en JSON.
- Imágenes validadas por extensión (`.jpg`, `.jpeg`, `.png`, `.webp`) y tamaño (5 MB).

**Web API**
- Endpoints REST de libros, afiliados y préstamos autenticados con **JWT**, separados de la cookie de la web.
- Login de la API con los mismos usuarios y contraseñas hasheadas; el token lleva los mismos claims que la cookie (id, rol).
- 401 y 403 en JSON (token faltante, inválido o vencido; rol sin permiso).
- Las reglas de préstamos (sanción, suscripción, multa) están en `ServicioPrestamo` y las usan la página de Vue y la API.
- Colección de pruebas en `docs/postman/`.

**Código**
- Helpers compartidos: `Archivos` (imágenes), `Paginacion` y `UsuarioActual`.
- Parciales `_Paginacion`, `_Alertas` y `_ModalConfirmacion` usados en todas las vistas.

### Pendiente respecto de la propuesta original

- Renovar un préstamo (crear uno nuevo para el mismo afiliado y ejemplar).
- Límite de préstamos simultáneos por afiliado.
- Informes: libros más prestados en los últimos X días, préstamos que vencen en X días, préstamos vencidos, afiliados sancionados.