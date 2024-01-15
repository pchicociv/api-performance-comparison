# Usar la imagen oficial de Microsoft SQL Server para Linux
FROM mcr.microsoft.com/mssql/server:2022-latest

# Establecer variables de entorno para la configuración de SQL Server
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=P@ssw0rd

# Crear un directorio dentro del contenedor para almacenar los archivos de la base de datos
WORKDIR /var/opt/mssql/backup

# Copiar el archivo de copia de seguridad de tu base de datos y el script SQL al contenedor
COPY api_comparative_db.bak .
COPY restore-database.sql .

# Copiar el script de entrada
COPY entrypoint.sh /usr/src/app/

# Establecer el script de entrada
ENTRYPOINT ["/bin/bash", "/usr/src/app/entrypoint.sh"]

# Exponer el puerto en el que SQL Server escucha
EXPOSE 1433