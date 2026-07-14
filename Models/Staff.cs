using System.ComponentModel.DataAnnotations;

namespace StaffCoreRD.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Cedula { get; set; }

        [Required]
        public string Cargo { get; set; }

        [Required]
        public string Departamento { get; set; }

        [Required]
        [Range(23223, double.MaxValue, ErrorMessage = "Mínimo RD$23,223")]
        public decimal Salario { get; set; }

        public DateTime FechaIngreso { get; set; }

        public bool Activo { get; set; } = true;
    }
}