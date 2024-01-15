RESTORE FILELISTONLY 
FROM DISK = '/var/opt/mssql/backup/api_comparative_db.bak'

RESTORE DATABASE api_comparative_db
FROM DISK = '/var/opt/mssql/backup/api_comparative_db.bak'
WITH 
    MOVE 'api_comparative_db' TO '/var/opt/mssql/data/api_comparative_db.mdf',
    MOVE 'api_comparative_db_log' TO '/var/opt/mssql/data/api_comparative_db.ldf',
    REPLACE,
    RECOVERY