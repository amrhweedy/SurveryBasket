using Asp.Versioning;
using OpenApiWithDotNet9;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication().AddJwtBearer();


#region Enable Api Versioning

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");

}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

#endregion


#region open api configuration for api versioning

builder.Services
    .AddEndpointsApiExplorer()
    .AddOpenApiConfiguration();  //  enable api versioning in the OpenApi document dynamically


#endregion


// the OpenApi creates a documentation of the API like SwaggerGen but does not create a ui like SwaggerUi
// there are some packages we can install them to create a ui for the api

#region open api congiguration (add the versions in the OpenApi document manually and add Bearer security scheme) 
//builder.Services.AddOpenApi(options =>
//{
//    // transform the OpenApi document, change the title and then version name and description
//    options.AddDocumentTransformer((document, context, cancellationToken) =>
//    {
//        document.Info = new()
//        {
//            Title = "Survey Basket API",
//            Version = "v1",
//            Description = "Survey Basket Description"
//        };
//        return Task.CompletedTask;
//    });


//    // transform the Bearer security scheme
//    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

//});

#endregion

#region Enable Authorization for the OpenApi documentation
// the admins only have access to the documentation of OpenApi
//builder.Services.AddAuthorization(o =>
//{
//    o.AddPolicy("AdminOnlyPolicy", b => b.RequireRole("Admin"));
//});

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //.RequireAuthorization("AdminOnlyPolicy");


    // use ScalarUi instead of SwaggerUi  , the url is => http://localhost:5067/scalar/v1
    app.MapScalarApiReference();


    // use SwaggerUi to generate ui for the api
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    //});
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
