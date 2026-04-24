using System.ComponentModel.DataAnnotations;

namespace apirestfull.Models
{
    public class Login
    {
        // ID del usuario que realiza el movimiento
        public int User_id { get; set; }

        // Extensión desde la que se conecta el usuario (puede representar dispositivo o terminal)
        public int Extension { get; set; }

        // Tipo de movimiento: 1 = login, 0 = logout
        public int TipoMov { get; set; }

        // Fecha y hora en la que ocurre el evento
        public DateTime Fecha { get; set; }
    }
}