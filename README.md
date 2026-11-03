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
| **Administrador** | Todo lo del empleado, más gestión de usuarios. Puede dar de baja afiliados. |
| **Empleado** | Creación, lectura y edición de entidades mencionadas. |

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
| API | (falta) Web API REST con JWT |

---

## 3. Modelo de datos

![`/docs/der.png`](docs/DER.png)


### Tablas (13)

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
- **Auditoría:** `prestamo` y `pago` guardan `creado_por_user_id` y `anulado_por_user_id`.
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

1. El afiliado **no está sancionado**.
2. Tiene una **suscripción vigente y pagada**.
3. **No tiene préstamos vencidos** sin devolver.
4. El ejemplar está **disponible**.

Si todo se cumple, el préstamo y el cambio de estado del ejemplar a `Prestado` se guardan **en una transacción**: o se hacen las dos cosas, o ninguna.

### Devoluciones y multas

- Al devolver, se registra la fecha real y el ejemplar vuelve a `Disponible`.
- Si la devolución es posterior a la fecha pactada, se genera **automáticamente un pago de tipo `Multa`** asociado al préstamo ($100 por día de atraso, configurable en `PrestamosController`).
- Un préstamo cargado por error se puede **anular** mientras no esté devuelto; el ejemplar queda disponible otra vez.

---

## 5. Evidencias de requerimientos mínimos


| # | Requerimiento mínimo | Estado | Enlace al código | Evidencia |
|---|---|---|---|---|
| 1 | **6+ tablas y relaciones 1:N y N:M** (13 tablas; N:M Libro–Género; Préstamo depende de Afiliado y Ejemplar) | ✅ Cumplido | [`Models/`](src/Models) · [`Repositories/`](src/Repositories) · [`database/biblioteca_db.sql`](database/biblioteca_db.sql) | DER en [`docs/DER.png`](docs/DER.png) |
| 2 | **Login, roles y avatar** (cookies + claims; `[Authorize(Roles = "Administrador")]`; avatar en perfil y alta de usuario) | ✅ Cumplido | [`Program.cs` (autenticación)](src/Program.cs#L9) · [`UsuariosController.Login`](src/Controllers/UsuariosController.cs#L45) · [`UsuariosController.Perfil` (avatar)](src/Controllers/UsuariosController.cs#L118) · [`RepositorioUsuario.ValidarLogin`](src/Repositories/RepositorioUsuario.cs#L12) · [`Views/Usuarios/Login.cshtml`](src/Views/Usuarios/Login.cshtml) | Captura de pantalla de perfil con avatar en [`docs/evidencia_avatar.png`](docs/evidencia_avatar.png) |
| 3 | **Manejo de archivos** (portadas de libros, fotos de autores, avatares) | ✅ Cumplido | [`ImagenesController.GuardarArchivoAsync`](src/Controllers/ImagenesController.cs#L16) · [`LibrosController.Create` (portada)](src/Controllers/LibrosController.cs#L67) · [`AutoresController.Create` (foto)](src/Controllers/AutoresController.cs#L62) · [`Views/Libros/Create.cshtml`](src/Views/Libros/Create.cshtml) | Video corto / GIF mostrando subida de comprobante |
| 4 | **ABM con Vue + AJAX** (Préstamos: alta, listado, devolución y anulación sin recargar) | ✅ Cumplido | [`Views/Prestamos/Index.cshtml`](src/Views/Prestamos/Index.cshtml) · [`wwwroot/js/prestamos.js`](src/wwwroot/js/prestamos.js) · [`PrestamosController.cs`](src/Controllers/PrestamosController.cs) | Video corto / GIF del ABM dinámico |
| 5 | **Paginado por servidor** (`LIMIT/OFFSET` en SQL; se pide solo la página actual) | ✅ Cumplido | [`RepositorioPrestamo.ObtenerPaginado`](src/Repositories/RepositorioPrestamo.cs#L30) · [`RepositorioPago.ObtenerPaginado`](src/Repositories/RepositorioPago.cs#L11) · [`RepositorioAfiliado.ObtenerPaginado`](src/Repositories/RepositorioAfiliado.cs#L11) · [`RepositorioLibro.ObtenerPaginado`](src/Repositories/RepositorioLibro.cs#L15) | Capturas del inspector de red enviando page=1 y page=2 en [`docs/evidencia_paginado1.png`](docs/evidencia_paginado1.png) y [`docs/evidencia_paginado2.png`](docs/evidencia_paginado2.png)|
| 6 | **Selección asíncrona de entidades relacionadas** (afiliado y ejemplar al crear un préstamo, sin cargar todos los registros) | ✅ Cumplido | [`AfiliadosController.BuscarAsincrona`](src/Controllers/AfiliadosController.cs#L124) · [`PrestamosController.BuscarEjemplares`](src/Controllers/PrestamosController.cs#L59) · [`wwwroot/js/prestamos.js`](src/wwwroot/js/prestamos.js) | GIF buscando entidades en desplegable |
| 7 | **Web API REST con JWT** | 🟡 Completar | Falta completar | Captura de Postman con Bearer Token y colección |

---

## 6. Cómo ejecutarlo en local

### Pasos

1. Clonar el repositorio.
2. Crear la base de datos e importar el script:
   ```sql
   CREATE DATABASE biblioteca_db CHARACTER SET utf8mb4;
   ```
   Luego importar `db/biblioteca_db.sql` (desde phpMyAdmin o por consola).
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

---

## 7. Estado de avance y pendientes

### Hecho

- Catálogo completo (libros, ejemplares, autores, editoriales, géneros, nacionalidades).
- Afiliados, suscripciones y pagos con sus reglas.
- Préstamos y devoluciones con Vue, transacciones y multa automática.
- Auditoría de quién crea y anula pagos y préstamos.

### Pendiente respecto de la propuesta original

- Renovar un préstamo (crear uno nuevo para el mismo afiliado y ejemplar).
- Límite de préstamos simultáneos por afiliado.
- Calcular la columna **Sanción** desde los préstamos vencidos, como ya se hace con la suscripción.
- Informes: libros más prestados en los últimos X días, préstamos que vencen en X días, préstamos vencidos, afiliados sancionados.
- Vista de auditoría para el administrador (quién cargó o anuló cada operación).