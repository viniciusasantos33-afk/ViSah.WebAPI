using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ViSah.Data;
using ViSah.OpenApi;
using ViSah.Services;
using ViSah.Services.Media;
using ViSah.Interfaces;
using ViSah.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<JwtBearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<JwtBearerOperationSecurityTransformer>();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<IMusicService>(services =>
    services.GetRequiredService<CatalogService>());
builder.Services.AddScoped<ICatalogService>(services =>
    services.GetRequiredService<CatalogService>());
builder.Services.AddScoped<IAudioDeliveryService, AudioDeliveryService>();
builder.Services.AddScoped<IAudioManagementService, AudioManagementService>();
builder.Services.AddSingleton<ICloudinaryAudioService, CloudinaryAudioService>();
builder.Services.AddScoped<ICoverManagementService, CoverManagementService>();
builder.Services.AddSingleton<ICloudinaryImageService, CloudinaryImageService>();
builder.Services.AddCors(o => o.AddPolicy("Frontend",
    p => p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                       ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Configure Jwt:Key using User Secrets or an environment variable.");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must contain at least 32 bytes.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes(JwtBearerSecuritySchemeTransformer.SchemeName));
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
