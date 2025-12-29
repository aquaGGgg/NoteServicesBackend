using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Notes.Application.UseCases.Notes.CreateNote;
using Notes.Application.UseCases.Notes.DeleteNote;
using Notes.Application.UseCases.Notes.GetNote;
using Notes.Application.UseCases.Notes.ListNotes;
using Notes.Application.UseCases.Notes.UpdateNote;
using Notes.Infrastructure.Persistence;
using Notes.WebApi.Extensions;
using Notes.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// CORS
const string CorsPolicyName = "DefaultCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        // дл€ разработки можно так (широко)
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();

        // ≈сли хочешь "по уму" (prod), замени на:
        // policy.WithOrigins("https://your-frontend.com")
        //       .AllowAnyHeader()
        //       .AllowAnyMethod();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notes API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Opaque",
        In = ParameterLocation.Header,
        Description = "¬ставь токен. Swagger сам добавит 'Bearer '"
    });

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

// DI: auth + infra + current user
builder.Services.AddWebApi(builder.Configuration);

// handlers
builder.Services.AddScoped<CreateNoteHandler>();
builder.Services.AddScoped<ListNotesHandler>();
builder.Services.AddScoped<GetNoteHandler>();
builder.Services.AddScoped<UpdateNoteHandler>();
builder.Services.AddScoped<DeleteNoteHandler>();

var app = builder.Build();

// DB init (твой вариант)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var applied = await db.Database.GetAppliedMigrationsAsync();
    if (applied.Any())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}

// Middleware order
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
