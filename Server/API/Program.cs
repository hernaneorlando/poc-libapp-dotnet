using LibraryApp.API.Authors;
using LibraryApp.API.Gateway;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library App", Version = "v1" });
});

builder.Services.AddDbContext<SqlDataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString"));
});
builder.Services.AddTransient<IAuthorService, AuthorService>();

// Liveness check
builder.Services.AddHealthChecks().AddCheck("healthy", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SqlDataContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library App v1"));

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Liveness check
app.UseHealthChecks("/healthz", new HealthCheckOptions { Predicate = r => r.Name == "healthy" });

app.Run();
