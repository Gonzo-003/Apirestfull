using Microsoft.EntityFrameworkCore;
using apirestfull.Models;

namespace apirestfull.Data
{
    // Contexto de la base de datos que gestiona las entidades y su relación con SQL Server
    public class AppDbContext : DbContext
    {
        // Constructor que recibe la configuración (cadena de conexión, etc.)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tabla de registros de login y logout
        public DbSet<Login> Logins { get; set; }

        // Tabla de usuarios
        public DbSet<User> Users { get; set; }

        // Tabla de áreas
        public DbSet<Area> Areas { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>()
                .HasKey(l => new { l.User_id, l.Fecha });
        }
    }
}