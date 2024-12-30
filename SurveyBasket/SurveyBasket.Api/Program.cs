using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

//builder.Host.UseSerilog((context, configuration) =>
//{
//    configuration
//    .MinimumLevel.Information()
//    .WriteTo.Console();
//});

// if i need to read the serilog configuration from the appSettings.json


builder.Host.UseSerilog((context, configuration) =>
configuration.ReadFrom.Configuration(context.Configuration)
);


//builder.Services.AddResponseCaching();

//builder.Services.AddOutputCache(options =>
//{
//    options
//    .AddPolicy("Polls", x =>
//    x
//    .Cache()
//    .Expire(TimeSpan.FromSeconds(60))
//    .Tag("AvailableQuestions")
//    );
//});


builder.Services.AddMemoryCache();


var app = builder.Build();

if (app.Environment.IsDevelopment())  // it knows that we are in development mode through the environment variable which in the LaunchSettings.json
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();  // to redirect http to https, it means if i use http it will redirect to https


app.UseCors();  // it must come before the authentication 

app.UseAuthorization();

//app.UseResponseCaching();

app.UseOutputCache();

app.MapControllers(); //scans all controllers in the application and collects the routes defined in those controllers. When a request is sent, the routing system will match the request URL to one of the collected routes, and then direct the request to the appropriate controller and action that handles that route.

app.UseExceptionHandler();

app.Run();


// the services which i add for the builder are the the components which the application need to work
// in the middleware i tell the application how it works 