## SSO Backend

### 1. Configuración inicial

1.1. Copia el archivo `.env.template` a `.env`:

```bash
cp .env.template .env
```

2.1. Modifica las variables de entorno `.env`, los puertos designados para este proyecto son:

- Api 4551
- Postgres 4550

### 2. Levantar contenedores

2.1. Desarrollo

1. Asegurate de que el archivo .env esté configurado correctamente.
2. Ejecutar el sigueiinte comando

```
docker-compose up -d
```

2.2. Producción

1. Configura las variables de entorno en .env.
2. Ejecuta el siguiente comando

```
docker compose -f docker-compose.yaml up -d
```

### 3. Migraciones de base de datos

3.1. Crear una nueva migración

```
dotnet ef migrations add InitialSsoSetup
```

3.2. Actualizar la base de datos en desarrollo

```
dotnet ef database update
```

3.3. Exportar migraciones a un script

```
dotnet ef migrations script --output ../scripts/ScriptTables.sql
```

3.4. Aplicar migraciones en producción

1. Crea un archivo de migración siguiendo el formato descrito en `manual_scripts-versionados.sql`
2. Coloca el archivo en la carpeta `/migrations`.
3. Ejecuta el contenedor Flyway

```
docker-compose up flyway
```