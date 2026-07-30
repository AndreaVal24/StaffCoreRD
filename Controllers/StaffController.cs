using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffCoreRD.Data;
using StaffCoreRD.Models;

namespace StaffCoreRD.Controllers
{
    // Requiere autenticación para acceder a cualquier acción
    [Authorize]
    public class StaffController : Controller
    {
        // Inyectar el DbContext
        private readonly StaffDbContext _context;

        public StaffController(StaffDbContext context)
        {
            _context = context;
        }

        // ============================================
        // LECTURA (R): Index
        // Lista personal activo ordenado por Nombre
        // Acceso: Admin, RRHH, Viewer (todos autenticados)
        // ============================================
        public async Task<IActionResult> Index()
        {
            // Obtener todos los empleados activos, ordenados por Nombre
            var personal = await Task.FromResult(
                _context.Personal
                    .Where(s => s.Activo)
                    .OrderBy(s => s.Nombre)
                    .ToList()
            );

            return View(personal);
        }

        // ============================================
        // DETALLES (Details): Perfil completo del empleado
        // Muestra todos los datos en una vista dedicada
        // Acceso: Admin, RRHH, Viewer (todos autenticados)
        // ============================================
        public async Task<IActionResult> Details(int? id)
        {
            // Validar que se pasó un ID
            if (id == null)
            {
                return NotFound();
            }

            // Buscar el empleado
            var staff = await _context.Personal.FindAsync(id);

            // Si no existe, retornar NotFound
            if (staff == null)
            {
                return NotFound();
            }

            // Devolver vista con todos los datos
            return View(staff);
        }

        // ============================================
        // CREACIÓN (C): Create GET
        // Devuelve View con new Staff() para pre-enlazar campos vacíos
        // Acceso: Admin, RRHH
        // ============================================
        [Authorize(Roles = "Administrador,RRHH")]
        public IActionResult Create()
        {
            // Devolver vista con nuevo objeto Staff vacío
            return View(new Staff());
        }

        // ============================================
        // CREACIÓN (C): Create POST
        // Valida ModelState, agrega a BD, muestra TempData éxito
        // Acceso: Admin, RRHH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Create(Staff staff)
        {
            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
            {
                // Si hay errores, devolver View con los datos que ingresó
                return View(staff);
            }

            // Agregar el nuevo empleado a la BD
            _context.Personal.Add(staff);
            await _context.SaveChangesAsync();

            // Mensaje de éxito en TempData (se muestra en _Layout)
            TempData["Exito"] = $"Empleado {staff.Nombre} creado exitosamente.";

            // Redirigir a Index
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // ACTUALIZACIÓN (U): Edit GET
        // Obtiene el empleado por ID y muestra formulario pre-cargado
        // Acceso: Admin, RRHH
        // ============================================
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Edit(int? id)
        {
            // Validar que se pasó un ID
            if (id == null)
            {
                return NotFound();
            }

            // Buscar el empleado en la BD
            var staff = await _context.Personal.FindAsync(id);

            // Si no existe, retornar NotFound
            if (staff == null)
            {
                return NotFound();
            }

            // Devolver vista con los datos pre-cargados
            return View(staff);
        }

        // ============================================
        // ACTUALIZACIÓN (U): Edit POST
        // Valida ID, actualiza en BD
        // La Vista debe enviar Id como hidden input
        // Acceso: Admin, RRHH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,RRHH")]
        public async Task<IActionResult> Edit(int id, Staff staff)
        {
            // Verificar que el ID del URL coincida con el del formulario
            if (id != staff.Id)
            {
                return NotFound();
            }

            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
            {
                // Si hay errores, devolver View sin perder datos
                return View(staff);
            }

            // Marcar el objeto como modificado
            _context.Personal.Update(staff);
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["Exito"] = $"Empleado {staff.Nombre} actualizado exitosamente.";

            // Redirigir a Index
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // ELIMINACIÓN (D): Delete GET
        // Obtiene el empleado y muestra confirmación
        // Nunca elimina en GET, solo muestra datos
        // Acceso: Solo Admin
        // ============================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            // Validar que se pasó un ID
            if (id == null)
            {
                return NotFound();
            }

            // Buscar el empleado
            var staff = await _context.Personal.FindAsync(id);

            // Si no existe, retornar NotFound
            if (staff == null)
            {
                return NotFound();
            }

            // Devolver vista con los datos (para que confirme)
            return View(staff);
        }

        // ============================================
        // ELIMINACIÓN (D): Delete POST (DeleteConfirmed)
        // Elimina de la BD después de confirmar
        // Acceso: Solo Admin
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]  // POST va a Delete, no DeleteConfirmed
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Buscar el empleado
            var staff = await _context.Personal.FindAsync(id);

            if (staff != null)
            {
                // Eliminar de la BD
                _context.Personal.Remove(staff);
                await _context.SaveChangesAsync();

                // Mensaje de éxito
                TempData["Exito"] = $"Empleado {staff.Nombre} eliminado exitosamente.";
            }

            // Redirigir a Index
            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // ESTADÍSTICAS: Resumen por departamento
        // Muestra: total empleados, total nómina, promedio salarial
        // Acceso: Admin, RRHH, Viewer (todos autenticados)
        // ============================================
        public async Task<IActionResult> Estadisticas()
        {
            // Obtener los datos agrupados por departamento
            var estadisticas = await Task.FromResult(
                _context.Personal
                    .Where(s => s.Activo)  // Solo empleados activos
                    .GroupBy(s => s.Departamento)
                    .Select(g => new DepartamentoEstadisticaViewModel
                    {
                        Departamento = g.Key,
                        TotalEmpleados = g.Count(),
                        TotalNomina = g.Sum(s => s.Salario),
                        SalarioPromedio = g.Average(s => s.Salario)
                    })
                    .OrderBy(e => e.Departamento)
                    .ToList()
            );

            return View(estadisticas);
        }
    }
}