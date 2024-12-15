using MapsterMapper;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();//registers all controllers in the application. It identifies controllers either by naming convention (any class ending with Controller) or by using the [ApiController] attribute (or [Controller] attribute for MVC controllers).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IPollService, PollService>();

// Mapster
var mappingConfig = TypeAdapterConfig.GlobalSettings;
mappingConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IMapper>(new Mapper(mappingConfig));


var app = builder.Build();

if (app.Environment.IsDevelopment())  // it knows that we are in development mode through the environment variable which in the LaunchSettings.json
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();  // to redirect http to https, it means if i use http it will redirect to https

app.UseAuthorization();

app.MapControllers(); //scans all controllers in the application and collects the routes defined in those controllers. When a request is sent, the routing system will match the request URL to one of the collected routes, and then direct the request to the appropriate controller and action that handles that route.
app.Run();


// the services which i add for the builder are the the components which the application need to work
// in the middleware i tell the application how it works 