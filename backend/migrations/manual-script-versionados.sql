/*
Los archivos de migración deben seguir el siguiente formato:
-- V{n}__{nombre}.sql

-- Donde {n} es un número entero que indica la versión de la migración y {nombre} es una descripción breve de la migración.
-- Por ejemplo:

V1__initial_schema.sql
V2__add_new_table.sql
V3__update_column_type.sql


* Las migraciones deben ser independientes y no deben depender de otras migraciones.
Esto significa que cada migración debe ser capaz de ejecutarse por sí sola y no debe depender de la ejecución de otras migraciones.
Esto es importante para garantizar que las migraciones se puedan aplicar en cualquier orden y que no haya conflictos entre ellas.

* Además, las migraciones deben ser idempotentes, lo que significa que se pueden aplicar varias veces sin causar efectos secundarios no deseados.
Esto es importante para garantizar que las migraciones se puedan aplicar en cualquier entorno y que no haya problemas de compatibilidad entre diferentes versiones de la base de datos.

* Las migraciones deben ser probadas en un entorno de desarrollo antes de ser aplicadas en producción.
Esto es importante para garantizar que las migraciones no causen problemas en la base de datos y que se puedan aplicar sin problemas.

* Las migraciones deben ser documentadas y versionadas en un sistema de control de versiones.
Esto es importante para garantizar que las migraciones se puedan rastrear y revertir si es necesario.

* Las migraciones deben ser aplicadas en un entorno de producción solo después de haber sido probadas y aprobadas en un entorno de desarrollo.
Esto es importante para garantizar que las migraciones no causen problemas en la base de datos y que se puedan aplicar sin problemas.

* No se pueden reusar el número de las versiones de las migraciones porque se generan automáticamente y se almacenan en la base de datos historica flyway_schema_history.

* La primera migración muchas veces empieza con V2 porque la V1 es generada automáticamente por flyway al inicializar la base de datos.
*/