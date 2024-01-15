# API performance comparison

I'm trying to create a performance comparative between different API technologies.
Requisites:
- Uses OpenAPI
- Uses OpenTelemetry
- Is behind a reverse proxy
- Calls an stored procedure in SqlServer DB

Comparison metrics:
- Requests per second


To start a test, choose a sample and execute:

```
docker compose up
````

Test will run for 2 minutes.
After that you will find the results in "test-results" folder.

## Samples

### .NET 8 Minimal API 

This sample contains an .NET 8 Minimal API which calls a stored procedure in a SQL Server DB. The procedure waits for 1 second and returns.
The API service is behind an NGINX reverse proxy.
Both the API and NGINX are configured to send telemety to a Jaeger instance.
OTL Traces can be found at [http://localhost:16686](http://localhost:16686) 