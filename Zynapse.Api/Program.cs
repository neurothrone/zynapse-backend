using Microsoft.EntityFrameworkCore;
using Zynapse.Persistence.Postgres.Data;
using Zynapse.Persistence.Postgres.Interfaces;
using Zynapse.Persistence.Postgres.Repositories;
using Zynapse.Persistence.Postgres.Utils;
using Zynapse.Api.Endpoints;
using Zynapse.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ZynapseDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SupabaseConnection") ??
        throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.")
    )
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZynapseDbContext>();
    await ProductSeeder.SeedAsync(context, "Data/products.json");
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapProductEndpoints();

app.Run();