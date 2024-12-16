

namespace SurveyBasket.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {

        // Add services to the container.

        services.AddControllers(); //registers all controllers in the application. It identifies controllers either by naming convention (any class ending with Controller) or by using the [ApiController] attribute (or [Controller] attribute for MVC controllers).

        // swagger
        services.AddSwaggerServices();

        // services
        services.AddScoped<IPollService, PollService>();

        // FluentValidation
        services.AddFluentValidationConfig();

        // Mapster
        services.AddMapsterConfig();

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }


    public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));


        return services;
    }

    public static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());  // it scans the assembly and find all classes which inherit from AbstractValidator and register it as a service
        services.AddFluentValidationAutoValidation(); // it makes automatic validation

        return services;
    }

}
