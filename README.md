# API performance comparison

I'm trying to create a performance comparative between different API technologies.
Requisites:
- Uses OpenAPI
- Uses OpenTelemetry
- Is behind a reverse proxy
- Calls an stored procedure in SqlServer DB
- Uses JWT
- Uses cryptography to hash users passwords

Comparison metrics:
- Requests per second


To start a test, choose go to a sample folder and execute:

```
docker compose up
````

Each test will run for 2 minutes.
After that you will find the results in "test-results" folder.

## Samples

### .NET 8 Minimal API 

This sample contains an .NET 8 Minimal API with 3 methods and a locust test for each one:
- **CallProcedure**: calls a stored procedure in a SQL Server DB. The procedure waits for 1 second and returns.
- **CallProcedureWithJwt**: same as previous but with token validation.
- **CreateUser**: calculates a password hash with SHA256 and a random salt.

The API service is behind an NGINX reverse proxy.
Both the API and NGINX are configured to send telemety to a Jaeger instance.
Method in API requires a JWT token.
OTL Traces can be found at [http://localhost:16686](http://localhost:16686) 