using Asp.Versioning;
using Cls.Api.Middlewares;
using Cls.Api.Services;
using Cls.Application.Mapping;
using Cls.Infrastructure.Options;
using Cls.Infrastructure.Persistence;
using Cls.Shared.Contracts.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Services
  .AddApiVersioning(options =>
  {
      options.DefaultApiVersion = new ApiVersion(1, 0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
  })
  .AddApiExplorer(setup =>
  {
      setup.GroupNameFormat = "'v'VVV";
      setup.SubstituteApiVersionInUrl = true;
  });

var assemblies = new Assembly[]
{
    Assembly.Load("Cls.Application"),
    Assembly.Load("Cls.Domain"),
    Assembly.Load("Cls.Infrastructure"),
    Assembly.Load("Cls.Shared")
};

builder.Services.AddAutoMapper(cfg =>
{
    foreach (var asm in assemblies)
        cfg.AddProfile(new AutoDiscoveryProfile(asm));
});


builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(err => new { PropertyName = kvp.Key, err.ErrorMessage })).ToList();
        var pd = new ProblemDetails { Title = "Validation failed", Status = StatusCodes.Status422UnprocessableEntity, Detail = "One or more validation errors occurred.", Type = "https://httpstatuses.com/422" };
        pd.Extensions["errors"] = errors;
        return new ObjectResult(pd) { StatusCode = StatusCodes.Status422UnprocessableEntity };
    };
});

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IProfilePictureOrchestrator, ProfilePictureOrchestrator>();
builder.Services.AddScoped<IOrderFileOrchestrator, OrderFileOrchestrator>();

Cls.Application.DependencyInjection.AddApplication(builder);
Cls.Infrastructure.DependencyInjection.AddInfrastructure(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cls API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
    }});
});

builder.Services.AddControllers();

builder.Services.AddCors(cfg => cfg.AddPolicy("GeneralPolicy", policy =>
{
    policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
}));

builder.Services
    .AddOptions<ObjectStorageOptions>()
    .Bind(builder.Configuration.GetSection("ObjectStorage"))
    .ValidateDataAnnotations()
    .Validate(o => o.Provider is "minio",
        "ObjectStorage:Provider must be 'minio'.")
    .ValidateOnStart();

builder.Services
    .AddOptions<MinioOptions>()
    .Bind(builder.Configuration.GetSection("Minio"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ObjectStorageOptions>>(sp =>
    new ValidateObjectStorageProviderOptions(
        sp.GetRequiredService<IConfiguration>()));

var app = builder.Build();

var conn = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException("ConnectionStrings:Default is missing.");

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlerMiddleware>();

await DbInitializer.MigrateAsync(app.Services);
app.UseCors("GeneralPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserCheckerMiddleware>();
app.MapControllers();
app.Run();

public partial class Program;
