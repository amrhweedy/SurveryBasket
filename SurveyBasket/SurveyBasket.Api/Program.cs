var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())  // it knows that we are in development mode through the environment variable which in the LaunchSettings.json
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  // to redirect http to https, it means if i use http it will redirect to https

app.UseAuthorization();

app.MapControllers();

app.Run();


// the services which i add for the builder are the the components which the application need to work
// in the middleware i tell the application how it works 