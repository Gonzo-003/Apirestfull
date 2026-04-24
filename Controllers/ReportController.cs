using Microsoft.AspNetCore.Mvc;
using apirestfull.Data;
using System.Text;
using apirestfull.Models;

namespace apirestfull.Controllers
{
    [ApiController]
    [Route("report")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("csv")]
        public IActionResult GetCsv()
        {
            var users = _context.Users.ToList();
            var areas = _context.Areas.ToList();
            var logins = _context.Logins
                .OrderBy(l => l.User_id)
                .ThenBy(l => l.Fecha)
                .ToList();

            var result = new List<ReportItem>();

            

            foreach (var user in users)
            {
                var userLogins = logins
                    .Where(l => l.User_id == user.User_id)
                    .OrderBy(l => l.Fecha)
                    .ToList();

                double totalHours = 0;

                for (int i = 0; i < userLogins.Count; i++)
                {
                    var current = userLogins[i];

                    // Solo si es login
                    if (current.TipoMov == 1)
                    {
                        // Buscar el siguiente logout válido
                        var nextLogout = userLogins
                            .Skip(i + 1)
                            .FirstOrDefault(l => l.TipoMov == 0);

                        if (nextLogout != null)
                        {
                            var diff = nextLogout.Fecha - current.Fecha;

                            // evitar negativos por datos sucios
                            if (diff.TotalSeconds > 0)
                                totalHours += diff.TotalHours;
                        }
                    }
                }

                var area = areas.FirstOrDefault(a => a.IDArea == user.IDArea);

                var nombreCompleto = $"{user.Nombres} {user.ApellidoPaterno} {user.ApellidoMaterno}";

                result.Add(new ReportItem
                {
                    Login = user.Login,
                    NombreCompleto = nombreCompleto,
                    Area = area?.AreaName ?? "N/A",
                    HorasTrabajadas = Math.Round(totalHours, 2)
                });

                Console.WriteLine($"--- Usuario {user.User_id} ---");

                for (int i = 0; i < userLogins.Count; i++)
                {
                    var l = userLogins[i];
                    Console.WriteLine($"{l.TipoMov} - {l.Fecha}");
                }

                Console.WriteLine($"Total movimientos: {userLogins.Count}");
            }

            var csv = new StringBuilder();
            csv.AppendLine("Login,NombreCompleto,Area,HorasTrabajadas");

            foreach (var r in result)
            {
                csv.AppendLine($"{r.Login},{r.NombreCompleto},{r.Area},{r.HorasTrabajadas}");
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "reporte.csv"
            );
        }
    }
}