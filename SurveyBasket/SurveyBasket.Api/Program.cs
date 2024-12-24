var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

 
var app = builder.Build();

if (app.Environment.IsDevelopment())  // it knows that we are in development mode through the environment variable which in the LaunchSettings.json
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  // to redirect http to https, it means if i use http it will redirect to https


app.UseCors();  // it must come before the authentication 


app.UseAuthorization();
 

app.MapControllers(); //scans all controllers in the application and collects the routes defined in those controllers. When a request is sent, the routing system will match the request URL to one of the collected routes, and then direct the request to the appropriate controller and action that handles that route.
app.Run();


// the services which i add for the builder are the the components which the application need to work
// in the middleware i tell the application how it works 