using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var apiRequestExample = new ApiRequest
{
    WaitSeconds = 1,
    ProcedureName = "WaitForIt"
};

builder.Services.AddSwaggerGen(c =>
{
    c.MapType<ApiRequest>(() => new OpenApiSchema
    {
        Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(apiRequestExample))
    });
});

const string serviceName = "net-8-minimal-api";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
var otlEndpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_EXPORTER_ENDPOINT") ??
                          "http://localhost:4317");

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddOtlpExporter();
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = otlEndpoint;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = otlEndpoint;
        }));

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
    exceptionHandlerApp.Run(async context =>
        await Results.Problem().ExecuteAsync(context)));

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/Api/Test", async (ApiRequest request) =>
{
    await using SqlConnection conn = new(connectionString);

    IEnumerable<StoredProcedureResult> result =
        await conn.QueryAsync<StoredProcedureResult>(request.ProcedureName, new { request.WaitSeconds }, commandType: CommandType.StoredProcedure);

    StoredProcedureResult? storedProcedureResult = result.FirstOrDefault();

    if (storedProcedureResult == null)
        return Results.BadRequest("Stored Procedure did not return any value");

    if (storedProcedureResult.ErrorCode != 0)
        return Results.BadRequest($"Stored Procedure returned an error: {storedProcedureResult.ErrorDescription}");

    ApiResponse apiResponse = new()
    {
        Id = Guid.NewGuid(),
        SomeString = storedProcedureResult.SomeString,
        SomeDecimal = storedProcedureResult.SomeDecimal,
        SomeInteger = storedProcedureResult.SomeInteger
    };
    return Results.Ok(apiResponse);
})
.Accepts<ApiRequest>("application/json")
.Produces<ApiResponse>()
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError)
.WithName("Test")
.WithOpenApi();

app.Run();

internal class ApiRequest
{
    public required string ProcedureName { get; set; }
    public int WaitSeconds { get; set; }
}

internal class StoredProcedureResult
{
    public string? SomeString { get; set; }
    public decimal? SomeDecimal { get; set; }
    public int SomeInteger { get; set; }
    public int ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
}

public class ApiResponse
{
    public Guid Id { get; set; }
    public string? SomeString { get; set; }
    public int? SomeInteger { get; set; }
    public decimal? SomeDecimal { get; set; }
}