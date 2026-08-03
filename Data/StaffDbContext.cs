using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffCoreRD.Models;

namespace StaffCoreRD.Data
{
    public class StaffDbContext : IdentityDbContext<IdentityUser>
    {
        public StaffDbContext(DbContextOptions<StaffDbContext> options) : base(options)
        {
        }

        public DbSet<Staff> Personal { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Seed: 2 empleados
            mb.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1,
                    Nombre = "Axel Chupani Acevedo",
                    Cedula = "001-4365218-7",
                    Cargo = "Analista de Sistemas",
                    Departamento = "Tecnología",
                    Salario = 75000,
                    FechaIngreso = new DateTime(2022, 3, 15),
                    Activo = true
                },
                new Staff
                {
                    Id = 2,
                    Nombre = "Andrea Valecillos Bolaños",
                    Cedula = "002-0000002-0",
                    Cargo = "Especialista en Nómina",
                    Departamento = "Recursos Humanos",
                    Salario = 58000,
                    FechaIngreso = new DateTime(2021, 6, 20),
                    Activo = true
                }
            );
        }
    }
}