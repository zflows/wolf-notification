IF NOT EXISTS (
	SELECT  schema_name
	FROM    information_schema.schemata
	WHERE   schema_name = 'notif' ) 
BEGIN
	EXEC sp_executesql N'create schema [notif]'
END
go
