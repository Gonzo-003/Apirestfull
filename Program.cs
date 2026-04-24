using Microsoft.EntityFrameworkCore;
using apirestfull.Data;
using System.Data;
using ExcelDataReader;
using apirestfull.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Conexión a SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// habilitar endpoints
app.MapControllers();

//importar areas
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     await ImportAreas(context);
// }

//importar tabala users
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     await ImportUsers(context);
// }


//Importar Logins
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //await ImportLogins(context);
}

app.Run();

//Importar tabla Areas
// async Task ImportAreas(AppDbContext context)
// {
//     System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

//     using var stream = File.Open("C:\\Users\\gonza\\Downloads\\CCenterRIA.xlsx", FileMode.Open, FileAccess.Read);
//     using var reader = ExcelReaderFactory.CreateReader(stream);

//     var result = reader.AsDataSet();

//     // 
//     var table = result.Tables["ccRIACat_Areas"];

//     if (table == null)
//     {
//         throw new Exception("No se encontró la hoja Areas");
//     }

//     if (context.Areas.Any())
//     {
//         return;
//     }

//     var areas = new List<Area>();

//     foreach (DataRow row in table.Rows)
//     {
//         // Saltar encabezado
//         if (row[0].ToString() == "IDArea")
//             continue;

//         // Saltar filas vacías
//         if (string.IsNullOrWhiteSpace(row[0]?.ToString()))
//             continue;

//         var id = Convert.ToInt32(row[0]);

//         // Evitar duplicados en memoria
//         if (areas.Any(a => a.IDArea == id))
//             continue;

//         areas.Add(new Area
//         {
            
//             AreaName = row[1]?.ToString(),
//             StatusArea = Convert.ToInt32(row[2]),
//             CreateDate = DateTime.TryParse(row[3]?.ToString(), out var fecha)
//                 ? fecha
//                 : DateTime.Now
//         });
//     }

//     context.Areas.AddRange(areas);
//     await context.SaveChangesAsync();
// }

//importar tabla ccUsers
// async Task ImportUsers(AppDbContext context)
// {
//     System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

//     using var stream = File.Open("C:\\Users\\gonza\\Downloads\\CCenterRIA.xlsx", FileMode.Open, FileAccess.Read);
//     using var reader = ExcelReaderFactory.CreateReader(stream);

//     var result = reader.AsDataSet();

//     var table = result.Tables["ccUsers"]; 

//     if (table == null)
//         throw new Exception("No se encontró la hoja Users");

//     if (context.Users.Any())
//         return;

//     var users = new List<User>();

//     foreach (DataRow row in table.Rows)
//     {
//         Console.WriteLine(string.Join(" | ", row.ItemArray));
//         if (row[0].ToString() == "User_id")
//             continue;

//         if (string.IsNullOrWhiteSpace(row[0]?.ToString()))
//             continue;

//         var id = Convert.ToInt32(row[0]);

//         if (users.Any(u => u.User_id == id))
//             continue;

//         users.Add(new User
//         {
//             User_id = id,
//             Login = row[1]?.ToString() ?? "",
//             Nombres = row[2]?.ToString() ?? "",
//             ApellidoPaterno = row[3]?.ToString() ?? "",
//             ApellidoMaterno = row[4]?.ToString(),
//             Password = row[5]?.ToString() ?? "",

//             TipoUser_id = int.TryParse(row[6]?.ToString(), out var tipo) ? tipo : 0,

//             Status = int.TryParse(row[7]?.ToString(), out var status) ? status : 0, 

//             fCreate = row[8] is DateTime fecha ? fecha : DateTime.Now,

//             IDArea = int.TryParse(row[9]?.ToString(), out var area) ? area : 0,

//             LastLoginAttempt = row[10] is DateTime last ? last : null
//         });
//     }

//     // activar identity insert
//     using var transaction = await context.Database.BeginTransactionAsync();

//     await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users ON");

//     context.Users.AddRange(users);
//     await context.SaveChangesAsync();

//     await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users OFF");

//     await transaction.CommitAsync();
// }


//Importar logins
async Task ImportLogins(AppDbContext context)
{
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    using var stream = File.Open("C:\\Users\\gonza\\Downloads\\CCenterRIA.xlsx", FileMode.Open, FileAccess.Read);
    using var reader = ExcelReaderFactory.CreateReader(stream);

    var result = reader.AsDataSet();

    var table = result.Tables["ccloglogin"]; 

    if (table == null)
        throw new Exception("No se encontró la hoja Logins");

    if (context.Logins.Any())
        return;

    var validUserIds = context.Users
        .Select(u => u.User_id)
        .ToHashSet();

    var logins = new List<Login>();

    foreach (DataRow row in table.Rows)
    {
        if (row[0].ToString() == "User_id")
            continue;

        if (string.IsNullOrWhiteSpace(row[0]?.ToString()))
            continue;

        var userId = int.TryParse(row[0]?.ToString(), out var u) ? u : 0;

        if (!validUserIds.Contains(userId))
            continue;

        DateTime fecha;

        if (row[3] is DateTime f)
            fecha = f;
        else if (!DateTime.TryParse(row[3]?.ToString(), out fecha))
            continue;


        var tipo = int.TryParse(row[2]?.ToString(), out var t) ? t : 0;

        // Buscar último movimiento del usuario
        var lastLogin = logins
            .Where(l => l.User_id == userId)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefault();

        //  Evitar login sin logout previo
        if (lastLogin != null)
        {
            if (lastLogin.TipoMov == 1 && tipo == 1)
                continue;
        }

        //  Evitar logout sin login previo
        if (lastLogin == null && tipo == 0)
            continue;
        // evitar duplicados por PK compuesta
        if (logins.Any(l => l.User_id == userId && l.Fecha == fecha))
            continue;

        logins.Add(new Login
        {
            User_id = userId,
            Extension = int.TryParse(row[1]?.ToString(), out var ext) ? ext : 0,
            TipoMov = tipo,
            Fecha = fecha
        });
    }

    context.Logins.AddRange(logins);
    await context.SaveChangesAsync();
}