using System.ComponentModel.DataAnnotations;

namespace apirestfull.Models
{
    public class User
    {
        // Clave primaria del usuario
        [Key]
        public int User_id { get; set; }

        // Nombre de usuario para login (máximo 50 caracteres)
        [Required]
        [MaxLength(50)]
        public string Login { get; set; } = null!;

        // Nombres del usuario (obligatorio)
        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; } = null!;

        // Apellido paterno (obligatorio)
        [Required]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; } = null!;

        // Apellido materno (opcional, no siempre es requqerido)
        public string? ApellidoMaterno { get; set; }

        // Contraseña del usuario (para este ejercicio se guarda como texto)
        public string Password { get; set; } = null!;

        // Tipo de usuario (por ejemplo: admin, operador, etc.)
        public int TipoUser_id { get; set; }

        // Tipo de Status
        public int Status { get; set; }

        // Fecha de creación del usuario
        public DateTime fCreate { get; set; }

        // Área a la que pertenece el usuario
        public int IDArea { get; set; }

        // Último intento de login (puede ser null si nunca ha intentado ingresar)
        public DateTime? LastLoginAttempt { get; set; }
    }
}