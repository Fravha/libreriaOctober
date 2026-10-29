using Blazor.Server.Data;
using Blazor.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MovimientoLock>();

var proveedor = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
if (proveedor.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<LibreriaContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("LibreriaConnection")));
}
else
{
    builder.Services.AddDbContext<LibreriaContext>(o =>
        o.UseSqlite(builder.Configuration.GetConnectionString("LibreriaSqlite")));
}

// El cliente WebAssembly corre en un origen distinto al de la API.
builder.Services.AddCors(o => o.AddPolicy("ClienteBlazor", p => p
    .WithOrigins("http://localhost:5236", "https://localhost:7236")
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibreriaContext>();
    // Pensado para una BD NUEVA de la demo; no modifica automáticamente bases SQL existentes.
    db.Database.Migrate();
    if (builder.Configuration.GetValue<bool>("SeedDemo"))
        DemoSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("ClienteBlazor");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "OK", proyecto = "LSP" }));
app.Run();
