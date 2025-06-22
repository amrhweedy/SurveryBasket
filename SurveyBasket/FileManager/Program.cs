using System.Reflection;
using FileManager.Persistence;
using FileManager.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString)
);


builder.Services
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
    .AddFluentValidationAutoValidation();

builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// before dot net 9
//app.UseStaticFiles();

//  dot net 9 => it likes the app.UseStaticFiles(); but it optimizes the files
// we use this middleware to allow the user to access the file or image from the server without access a specific endpoint
// so if the user write this url in the browser => https://localhost:7253/Images/profilepicture.jpeg , this image will be displayed on the browser
app.MapStaticAssets();

app.Run();
