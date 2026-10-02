using System.Text;
using System.Text.Json.Serialization;
using EventTicketing.Business.Abstract;
using EventTicketing.Business.Concrete;
using EventTicketing.Core.DataAccess;
using EventTicketing.Core.Utilities.Security;
using EventTicketing.Core.Utilities.Security.Hashing;
using EventTicketing.Core.Utilities.Security.Jwt;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.DataAccess.Concrete.EntityFramework;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.WebAPI.Mapping;
using EventTicketing.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "posters"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventTicketingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()
    ?? throw new InvalidOperationException("TokenOptions section is missing.");

if (string.IsNullOrWhiteSpace(tokenOptions.SecurityKey) || tokenOptions.SecurityKey.Length < 32)
    throw new InvalidOperationException(
        "TokenOptions:SecurityKey must be configured with at least 32 characters.");

builder.Services.AddSingleton(tokenOptions);
builder.Services.AddSingleton<ITokenHelper, JwtHelper>();
builder.Services.AddSingleton<IPasswordHashingHelper, PasswordHashingHelper>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddScoped<IEventDal, EfEventDal>();
builder.Services.AddScoped<ITicketDal, EfTicketDal>();
builder.Services.AddScoped<IUserDal, EfUserDal>();
builder.Services.AddScoped<IReportingDal, EfReportingDal>();

builder.Services.AddScoped<IEventService, EventManager>();
builder.Services.AddScoped<ITicketService, TicketManager>();
builder.Services.AddScoped<IReportingService, ReportingManager>();
builder.Services.AddScoped<IAuthService, AuthManager>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "Paste the token returned by /api/auth/login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme),
            new List<string>()
        }
    });
});

const string CorsPolicy = "ClientCorsPolicy";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
