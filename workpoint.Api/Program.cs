using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using workpoint.Api;
using workpoint.Api.Middleware;
using workpoint.Application.Interfaces;
using workpoint.Application.Service;
using workpoint.Application.Services;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;
using workpoint.Infrastructure.Messaging;
using workpoint.Infrastructure.Repositories;
using workpoint.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------- SERVICES ----------------

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Booking
builder.Services.AddScoped<IBookingService, BookingService>();

// Auth / Users
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IAuthServices, AuthService>();

// Spaces
builder.Services.AddScoped<IRepository<Space>, SpaceRepository>();
builder.Services.AddScoped<ISpaceService, SpaceService>();

// Photos
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();


// Webhooks y RabbitMQ
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IWebhookSubscriptionRepository, WebhookSubscriptionRepository>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient();

// Consumer de webhooks (background service)
builder.Services.AddHostedService<WebhookConsumer>();

// --------- VALIDATION ERRORS (400) → WEBHOOK ---------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var webhookService = context.HttpContext.RequestServices
            .GetRequiredService<IWebhookService>();

        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        //  Webhook de validación (400)
        webhookService.SendWebhookAsync("validation.failed", new
        {
            statusCode = 400,
            path = context.HttpContext.Request.Path.Value,
            method = context.HttpContext.Request.Method,
            errors,
            timestamp = DateTime.UtcNow
        }).GetAwaiter().GetResult();

        return new BadRequestObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors
        });
    };
});

// Authentication JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WorkPoint API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese su token JWT. Ejemplo: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ------------- MIDDLEWARE PIPELINE -------------

//  500 → Webhook
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkPoint API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseCors("DevCorsPolicy");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();