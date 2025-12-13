using System.Text;
using FluentValidation;
using identity_service.Data;
using identity_service.Dtos.AuthAuditLog;
using identity_service.Dtos.Role;
using identity_service.Dtos.SystemRegistry;
using identity_service.Dtos.User;
using identity_service.Models;
using identity_service.Repositories;
using identity_service.Repositories.Interfaces;
using identity_service.Services;
using identity_service.Services.Interfaces;
using identity_service.Validations.AuthAuditLog;
using identity_service.Validations.Role;
using identity_service.Validations.SystemRegistry;
using identity_service.Validations.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Cargar la configuración adecuada según el entorno
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure connection string
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


    // Si es producción, las variables de entorno serán sustituidas automáticamente
    if (builder.Environment.IsProduction())
    {
        // Se pueden usar variables de entorno para sustituir los valores en la cadena de conexión
        connectionString = connectionString.Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
                                             .Replace("${DB_USERNAME}", Environment.GetEnvironmentVariable("DB_USERNAME"))
                                             .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD"));
    }
    Console.WriteLine($"Connection string: {connectionString}");
    options.UseNpgsql(connectionString);
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1"
    });

    // 🔐 Definición de seguridad
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token en formato: Bearer {token}"
    });

    // 🔐 Habilitar autorización global
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configurar el identity framework
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Add SignInManager and UserManager explicitly for dependency injection
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<ApplicationRole>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        var jwt = builder.Configuration.GetSection("JWTSettings");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["ValidIssuer"],
            ValidAudience = jwt["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Secret"]!)
            )
        };
    });    

// Inyección de dependencias

// Mappers
builder.Services.AddAutoMapper(typeof(Program));

// Validators
builder.Services.AddScoped<IValidator<AuthAuditLogForCreateDto>, AuthAuditLogForCreateValidator>();
builder.Services.AddScoped<IValidator<CreateRoleDto>, CreateRoleValidator>();
builder.Services.AddScoped<IValidator<UpdateRoleDto>, UpdateRoleValidator>();
builder.Services.AddScoped<IValidator<CreateSystemRegistryDto>, CreateSystemRegistryValidator>();
builder.Services.AddScoped<IValidator<UpdateSystemRegistryDto>, UpdateSystemRegistryValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();

// Repositories
builder.Services.AddScoped<IAuthAuditLogRepository, AuthAuditLogRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ISystemRegistryRepository, SystemRegistryRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthAuditLogService, AuthAuditLogService>();
builder.Services.AddScoped<ISystemRegistryService, SystemRegistryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRoleClaimEncoderService, RoleClaimEncoderService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CustomCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();