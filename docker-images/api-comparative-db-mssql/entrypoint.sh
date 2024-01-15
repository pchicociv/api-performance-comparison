#!/bin/bash
set -e

# Iniciar SQL Server en segundo plano
/opt/mssql/bin/sqlservr &

# Esperar a que SQL Server se inicie por completo (ajusta el tiempo según sea necesario)
sleep 20s

# Restaurar la base de datos
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "P@ssw0rd" -d master -i /var/opt/mssql/backup/restore-database.sql

# Mantener el contenedor en ejecución
tail -f /dev/null