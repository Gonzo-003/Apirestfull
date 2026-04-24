# API RESTful - Gestión de Logins (.NET 8 + SQL Server)

# Descripción
Este proyecto implementa una API RESTful en ASP.NET Core utilizando Entity Framework Core para gestionar registros de login y logout de usuarios.

Incluye:
- CRUD de logins
- Validaciones de negocio
- Importación de datos desde Excel
- Generación de reporte CSV con horas trabajadas

## 1. Levantar contenedor de SQL Server con Docker

Asegúrate de tener Docker instalado y ejecutándose.

Ejecuta el siguiente comando:

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong!Passw0rd'    -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest

Verifica que esté corriendo:

docker ps

## 2. Conectar a la base de datos

Puedes se reocmienda usar:

SQL Server Management Studio (SSMS)


Credenciales:

Server: localhost,1433
User: sa
Password: YourStrong!Passw0rd

## 4. Configurar conexión en la API

En appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=TestDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
}

## 5. Ejecutar migraciones

En la terminal del proyecto:

dotnet ef database update

Esto creará las tablas:

Users
Areas
Logins

### 5.1 Importar datos desde Excel (opcional)

El proyecto incluye lógica para importar datos desde el archivo `CCenterRIA.xlsx`.

Para ejecutar la importación:

1. Abre el archivo `Program.cs`
2. Ubica la sección:


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //await ImportLogins(context);
}

Descomenta la línea correspondiente:
await ImportLogins(context);

Ejecuta nuevamente la aplicación:
dotnet run

Importante:

La importación solo se ejecuta si la tabla está vacía
Evita duplicados automáticamente
Asegúrate de que la ruta del archivo Excel sea correcta

Después de ejecutar, puedes volver a comentar la línea para evitar reimportaciones innecesarias.

## 6. Ejecutar la API

La API estará disponible en:

http://localhost:5177

Swagger:

http://localhost:5177/swagger

## 7. Endpoints disponibles
 GET /logins

Obtiene todos los registros de login/logout

 POST /logins

Crea un nuevo login/logout

Ejemplo:

{
  "user_id": 1,
  "extension": 123,
  "tipoMov": 1,
  "fecha": "2025-10-03T13:00:00"
}

Validaciones:

    User_id debe existir
    Fecha no puede ser futura
    TipoMov solo puede ser:
        1 = login
        0 = logout
    No permite:
        login sin logout previo
        logout sin login previo

# PUT /logins/{user_id}/{fecha}

Actualiza un registro existente

# DELETE /logins/{user_id}/{fecha}

Elimina un registro

##  8. Consultas SQL (Ejercicio 2)

###  Objetivo

Calcular el usuario que ha pasado más tiempo logueado a partir de eventos de login/logout almacenados en la tabla `Logins`.

---

###  Estructura de la tabla

- `User_id`: identificador del usuario  
- `Fecha`: fecha y hora del evento  
- `TipoMov`: tipo de evento  
  - 1 → login  
  - 0 → logout  

---

###  Lógica aplicada

Los datos no representan sesiones completas, sino eventos individuales.  
Por ello, el proceso consiste en:

1. Ordenar los eventos por usuario y fecha  
2. Emparejar cada login con su logout usando funciones de ventana  
3. Filtrar únicamente pares válidos (login seguido de logout)  
4. Calcular la duración de cada sesión  
5. Agregar resultados según el caso (suma o promedio)  

Se utilizó la función `LEAD()` para obtener el siguiente evento dentro del mismo usuario.

###  Usuario con mayor tiempo logueado

Se suman todas las sesiones por usuario y se obtiene el mayor

Usuario con menor tiempo logueado

Misma lógica, cambiando el orden:
ORDER BY total_segundos ASC;

Promedio de tiempo logueado por mes

Se calcula la duración por sesión y luego se agrupa por:

Usuario
Año
Mes

AVG(duracion_segundos)

Consideraciones:
Logins sin logout son ignorados
Solo se consideran pares válidos (1 → 0)
Se asume orden correcto por fecha
Se evita contar sesiones incompletas


## 9. Generar CSV (Ejercicio 3)
Endpoint:
GET /report/csv
Funcion:

Genera un reporte con:

Login (usuario)
Nombre completo
Área
Horas trabajadas
Lógica:
Empareja login (1) con logout (0)
Calcula tiempo trabajado por usuario
Suma horas totales

## 10. Descargar CSV

El endpoint devuelve directamente un archivo CSV descargable.

# Opción 1: Navegador

Abrir:

http://localhost:5177/report/csv

Se descarga automáticamente reporte.csv.

# Opción 2: Postman
Método: GET
URL:
http://localhost:5177/report/csv
Click en Send
Guardar respuesta como archivo

# Opción 3: curl
curl -o reporte.csv http://localhost:5177/report/csv

## 10. Pruebas recomendadas
#Validaciones
Crear login sin logout → error
Crear logout sin login → error
Fecha futura → error
User_id inexistente → error

###  Pruebas con curl en PowerShell

Para probar el endpoint de generación de CSV desde la terminal, se utilizó `curl`.

Nota: En PowerShell se recomienda usar `curl.exe`.

#### Descargar el archivo CSV

comado: 
curl.exe -o reporte.csv http://localhost:5177/report/csv


# Flujo correcto
POST login (tipoMov = 1)
POST logout (tipoMov = 0)
Verificar horas en /report/csv

# Estructura del proyecto
/Controllers
  - LoginsController.cs
  - ReportController.cs

/Data
  - AppDbContext.cs

/Models
  - Login.cs
  - User.cs
  - Area.cs
  - ReportItem.cs

Program.cs
appsettings.json

## Notas importantes

Se usa clave compuesta en Logins:

###  Clave primaria compuesta en Logins

Se definió una clave primaria compuesta en la tabla `Logins` utilizando:

User_id + Fecha

Motivo:
- Un usuario puede tener múltiples registros de login/logout haciendo que el `User_id`se repita, por ende no puede ser key
- `Fecha` permite distinguir cada evento en el tiempo
- La combinación evita duplicados exactos de eventos

Esto garantiza integridad de datos sin necesidad de un ID artificial.


Se evita duplicados
Se valida consistencia de eventos (login/logout)
Usuarios sin registros de login → horas trabajadas = 0