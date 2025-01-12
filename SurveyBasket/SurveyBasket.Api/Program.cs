using Hangfire;
using HangfireBasicAuthenticationFilter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SurveyBasket.Api.Services.BackgroundJobs;

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


builder.Services.AddDistributedMemoryCache();


var app = builder.Build();

if (app.Environment.IsDevelopment())  // it knows that we are in development mode through the environment variable which in the LaunchSettings.json
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();  // to redirect http to https, it means if i use http it will redirect to https

app.UseHangfireDashboard("/jobs", new DashboardOptions()
{
    // if we need to make authentication for the dashboard install package Hangfire.Dashboard.Basic.Authentication
    Authorization = [
        new HangfireCustomBasicAuthenticationFilter{
            User = builder.Configuration.GetValue<string>("HangfireSettings:Username"),
            Pass = builder.Configuration.GetValue<string>("HangfireSettings:Password")
        }
        ],
    DashboardTitle = "Survey Basket Dashboard", // change the title of the dashboard

    // now i can go to the dashboard and select any job and run it manually or delete it 
    // so if need to make all these jobs readonly we can use the IsReadOnlyFunc property
    // so i can not delete or run any job manually

    // IsReadOnlyFunc = (DashboardContext context)=> true,
});

// configure Recurring Job

//var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
//using var scope = scopeFactory.CreateScope();
//var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
//RecurringJob.AddOrUpdate("SendNewPollsNotification", () => notificationService.SendNewPollsNotification(null), Cron.Daily);

// this job will run every day at 12:00 am
// if we to run the job at a specific time we can use the expression 
// https://crontab.guru/  => this site will help us to generate the cron expression 
// we put the job in the program because when we run the appliction this job will be added to the RecurringJobs 
// and when we open the dashboard we can select this job and run it manually now

RecurringJob.AddOrUpdate<INotificationService>("SendNewPollsNotification", x => x.SendNewPollsNotification(null), Cron.Daily);


app.UseCors();  // it must come before the authentication 

app.UseAuthorization();

app.MapControllers(); //scans all controllers in the application and collects the routes defined in those controllers. When a request is sent, the routing system will match the request URL to one of the collected routes, and then direct the request to the appropriate controller and action that handles that route.

app.UseExceptionHandler();

//HealthCheckOptions => it displays the health of the application and its dependencies like the database in a readable format like json
// and gives information about the status of every dependency like the database
// when you visit http://localhost:5000/health, it will return the status of all health checks (like database connections, etc.), and that information will be formatted for the
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("health-check-api", new HealthCheckOptions
{
    Predicate = x => x.Tags.Contains("api"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


app.Run();


// the services which i add for the builder are the the components which the application need to work
// in the middleware i tell the application how it works 