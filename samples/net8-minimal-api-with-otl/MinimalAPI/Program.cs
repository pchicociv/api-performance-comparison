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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;


const string SERVICE_NAME = "net-8-minimal-api";

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var callProcedureRequestExample = new CallProcedureRequest
{
    WaitSeconds = 1,
    ProcedureName = "WaitForIt"
};

var createUserRequestExample = new CreateUserRequest
{
    Username = "MyUsername",
    Password = "MyStr0ngP@ssw0rd"
};

builder.Services.AddSwaggerGen(options =>
{
    options.MapType<CallProcedureRequest>(() => new OpenApiSchema
    {
        Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(callProcedureRequestExample))
    });
    options.MapType<CreateUserRequest>(() => new OpenApiSchema
    {
        Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(createUserRequestExample))
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "Copy the following token into the textbox (including 'Bearer'): Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJOYW1lIjoicGNoaWNvY2l2IiwiaWF0IjoxNzA1NTEzNTAzfQ.E5_l2y6OGMt8FAwcnaYLpbUeDsBwersJAmLup7G2aOM",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }

            },
            new List<string>()
        }
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
var otlEndpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_EXPORTER_ENDPOINT") ??
                          "http://localhost:4317");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "3a1f27f794d37ed1f61ba3e11f794e72fd16a614aa96f1c1a8b2198007f74bab";

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(SERVICE_NAME))
        .AddOtlpExporter();
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(SERVICE_NAME))
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "http://minimalapi.net",
        ValidAudience = "http://audience.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
    exceptionHandlerApp.Run(async context =>
        await Results.Problem().ExecuteAsync(context)));


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/Api/CallProcedure", async (CallProcedureRequest request) =>
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
    .Accepts<CallProcedureRequest>("application/json")
    .Produces<ApiResponse>()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithName("CallProcedure")
    .WithOpenApi();

app.MapPost("/Api/CallProcedureWithJwt", async (CallProcedureRequest request) =>
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
    .RequireAuthorization()
    .Accepts<CallProcedureRequest>("application/json")
    .Produces<ApiResponse>()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithName("CallProcedureWithJwt")
    .WithOpenApi();

app.MapPost("/Api/CreateUser", (CreateUserRequest request) => {

    byte[] salt = new byte[128 / 8];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }

    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: request.Password,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

    return Results.Created($"/users/{request.Username}", new { Usuario = request.Username, HashedPassword = hashed });


    })
    .Accepts<CreateUserRequest>("application/json")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithName("CreateUser")
    .WithOpenApi();

app.Run();


public class CreateUserRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
internal class CallProcedureRequest
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
