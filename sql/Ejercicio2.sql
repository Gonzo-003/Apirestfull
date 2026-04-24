-- CTE que transforma los eventos (login/logout) en pares de sesiones
-- usando la siguiente fila por cada usuario
WITH Movimientos AS (
    SELECT 
        User_id,  -- Identificador del usuario

        -- Fecha del evento actual (puede ser login o logout)
        fecha AS login_time,

        -- Fecha del siguiente evento del mismo usuario
        -- Se usa para emparejar login → logout
        LEAD(fecha) OVER (PARTITION BY User_id ORDER BY fecha) AS logout_time,

        -- Tipo de movimiento actual (1 = login, 0 = logout)
        Tipomov,

        -- Tipo de movimiento de la siguiente fila
        -- Sirve para validar que el siguiente evento sea un logout
        LEAD(Tipomov) OVER (PARTITION BY User_id ORDER BY fecha) AS next_tipo

    FROM dbo.Logins
),

-- CTE que calcula el tiempo total logueado por cada usuario
Totales AS (
    SELECT 
        User_id,  -- Identificador del usuario

        -- Suma total del tiempo en segundos entre login y logout
        SUM(DATEDIFF(SECOND, login_time, logout_time)) AS total_segundos

    FROM Movimientos

    WHERE 
        -- Solo se consideran filas donde el evento actual es login
        Tipomov = 1 

        -- Y el siguiente evento es logout (asegura pares válidos)
        AND next_tipo = 0

    -- Agrupa por usuario para obtener su tiempo total
    GROUP BY User_id
)

-- Consulta final: obtiene el usuario con mayor tiempo total logueado
SELECT TOP 1
    User_id,  -- Usuario con más tiempo acumulado

    -- Convierte el total de segundos a un formato legible
    CONCAT(
        'Tiempo total: ',

        -- Días completos
        total_segundos / 86400, ' días, ',

        -- Horas restantes después de quitar días
        (total_segundos % 86400) / 3600, ' horas, ',

        -- Minutos restantes después de quitar horas
        (total_segundos % 3600) / 60, ' minutos, ',

        -- Segundos restantes
        total_segundos % 60, ' segundos'
    ) AS TiempoTotal

FROM Totales

-- Ordena de mayor a menor para obtener el usuario con más tiempo
ORDER BY total_segundos DESC;





-- Consulta del usuario que menos tiempo ha estado logueado
WITH Movimientos AS (
    SELECT 
        User_id,

        -- Fecha del evento actual
        fecha AS login_time,

        -- Fecha del siguiente evento del mismo usuario
        LEAD(fecha) OVER (
            PARTITION BY User_id 
            ORDER BY fecha
        ) AS logout_time,

        -- Tipo actual (1 = login, 0 = logout)
        Tipomov,

        -- Tipo del siguiente evento
        LEAD(Tipomov) OVER (
            PARTITION BY User_id 
            ORDER BY fecha
        ) AS next_tipo

    FROM dbo.Logins
),

-- CTE que suma el tiempo total por usuario
Totales AS (
    SELECT 
        User_id,

        -- Suma del tiempo en segundos entre login y logout
        SUM(DATEDIFF(SECOND, login_time, logout_time)) AS total_segundos

    FROM Movimientos
    WHERE 
        Tipomov = 1      -- solo eventos de login
        AND next_tipo = 0 -- aseguramos que el siguiente sea logout
    GROUP BY User_id
)

-- Consulta final: usuario con MENOR tiempo logueado
SELECT TOP 1
    User_id,

    -- Formato legible del tiempo
    CONCAT(
        'Tiempo total: ',
        total_segundos / 86400, ' días, ',
        (total_segundos % 86400) / 3600, ' horas, ',
        (total_segundos % 3600) / 60, ' minutos, ',
        total_segundos % 60, ' segundos'
    ) AS TiempoTotal

FROM Totales

-- cambio de orden, en comparacion al anterior
ORDER BY total_segundos ASC;



-- Promedio de logueo por mes
WITH Movimientos AS (
    SELECT 
        User_id,
        fecha AS login_time,
        LEAD(fecha) OVER (
            PARTITION BY User_id 
            ORDER BY fecha
        ) AS logout_time,
        Tipomov,
        LEAD(Tipomov) OVER (
            PARTITION BY User_id 
            ORDER BY fecha
        ) AS next_tipo
    FROM dbo.Logins
),

Sesiones AS (
    SELECT 
        User_id,

        -- Año y mes del login
        YEAR(login_time) AS year, -- sql no maneje la "ñ"
        MONTH(login_time) AS mes,

        -- Duración de cada sesión en segundos
        DATEDIFF(SECOND, login_time, logout_time) AS duracion_segundos

    FROM Movimientos
    WHERE 
        Tipomov = 1 
        AND next_tipo = 0
)

SELECT 
    User_id,
    year,
    mes,

    -- Promedio en segundos
    AVG(duracion_segundos) AS promedio_segundos,

    -- Formato legible
    CONCAT(
        AVG(duracion_segundos) / 86400, ' días, ',
        (AVG(duracion_segundos) % 86400) / 3600, ' horas, ',
        (AVG(duracion_segundos) % 3600) / 60, ' minutos, ',
        AVG(duracion_segundos) % 60, ' segundos'
    ) AS PromedioTiempo

FROM Sesiones
GROUP BY 
    User_id, year, mes
ORDER BY 
    User_id, year, mes;