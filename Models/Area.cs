using System.ComponentModel.DataAnnotations;

namespace apirestfull.Models
{
    public class Area
    {
        // Clave primaria del área
        [Key]
        public int IDArea { get; set; }

        // Nombre del área (ej: soporte, ventas, etc.)
        public string? AreaName { get; set; }

        // Estado del área (por ejemplo: 1 = activa, 0 = inactiva)
        public int StatusArea { get; set; }

        // Fecha de creación del área
        public DateTime CreateDate { get; set; }
    }
}