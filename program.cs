using Microsoft.EntityFrameworkCore;
using SarawakTourism.Data;
using SarawakTourismApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register custom services
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve homepage.html as the default file at root
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "homepage.html" }
});
app.UseStaticFiles();

app.MapControllers();

app.Run();

