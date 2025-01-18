using Asp.Versioning.ApiExplorer;
using OpenApiWithDotNet9.OpenApiTransformers;

namespace OpenApiWithDotNet9;

// this class is used to enable api versioning in the OpenApi document
// this class is used to scan all versions in the api and add them in the open api documentation
public static class DependencyInjection
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var apiVersionDescriptionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {

            services.AddOpenApi(description.GroupName, options =>
            {
                // transform the Bearer security scheme
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

                options.AddDocumentTransformer((document, context, CancellationToken) =>
                {
                    document.Info = new()
                    {
                        Title = "SurveyBasket API",
                        Version = description.ApiVersion.ToString(),
                        Description = $"API Description.{(description.IsDeprecated ? " This API version has been deprecated." : string.Empty)}"

                    };
                    return Task.CompletedTask;
                });
            });

        }

        return services;
    }
}
