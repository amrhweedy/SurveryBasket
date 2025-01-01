using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Services.Authentication;
using SurveyBasket.Api.Services.Cashing;
using SurveyBasket.Api.Services.Emails;
using SurveyBasket.Api.Services.Results;
using SurveyBasket.Api.Services.Votes;
using SurveyBasket.Api.Settings;
using System.Text;

namespace SurveyBasket.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {

        // Add services to the container.

        services.AddControllers(); //registers all controllers in the application. It identifies controllers either by naming convention (any class ending with Controller) or by using the [ApiController] attribute (or [Controller] attribute for MVC controllers).

        // Cors

        string[] allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()!;

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
            });
        });


        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });


        // swagger
        services.AddSwaggerConfig();

        // services
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IEmailSender, EmailService>();


        services.AddHttpContextAccessor();

        // FluentValidation
        services.AddFluentValidationConfig();

        // Mapster
        services.AddMapsterConfig();

        // Authentication
        services.AddAuthenticationConfig(configuration);

        // Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // HangFire
        services.AddHangfireConfig(configuration);

        // Mail Settings Configuration => IOptions<MailSettings>
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));


        return services;
    }

    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
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

    private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());  // it scans the assembly and find all classes which inherit from AbstractValidator and register it as a service
        services.AddFluentValidationAutoValidation(); // it makes automatic validation

        return services;
    }


    private static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // configure the identity

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        // The JwtProvider likely does not maintain any per - user or per-request state; it simply generates JWT tokens based on the input(ApplicationUser).
        //Stateless services are ideal for singleton lifetime because they can be safely reused across multiple threads without side effects.

        services.AddSingleton<IJwtProvider, JwtProvider>();

        // It maps the values from the "Jwt" section of appsettings.json (or any configuration source) to the properties of the JwtOptions class.
        // It registers IOptions<JwtOptions> in the Dependency Injection (DI) container so that when I inject IOptions<JwtOptions> in any class, it will provide an instance of JwtOptions with the mapped values.
        // The registration behaves like a singleton, meaning the same JwtOptions object is used across the application unless you specifically use IOptionsSnapshot<JwtOptions>.

        // services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        JwtOptions? jwtSettings = configuration.GetSection("Jwt").Get<JwtOptions>();



        // Authentication configuration
        services.AddAuthentication(options =>
        {
            // this 2 configurations are used with the [authorize] attribute to avoid tell this attribute every time i use it that the token is jwt bearer or i will use the jwt bearer token to make authentication
            // because there are many ways to make authentication like basic authentication so we need to tell the application how to make authentication or any type we use to make authentication
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;  //if the token is valid it will save the token in property authentication property where i can access the token through the request i mean through the httpContext
                o.TokenValidationParameters = new TokenValidationParameters  // this is the important options because this represents how the system will validate the token or what are the information which i will validate it in the token
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)), // i tell the system to validate the signing key, the signing key which in the token must equal the singing key which in the app.settings or in the secret file
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    ClockSkew = TimeSpan.Zero  //The default value of ClockSkew is 5 minutes.That means if you haven't set it, your token will be still valid for up to 5 minutes. If you want to expire your token on the exact time; you'd need to set ClockSkew to zero 

                };
            });


        services.Configure<IdentityOptions>(options =>
        {

            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });



        return services;
    }

    private static IServiceCollection AddHangfireConfig(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));

        services.AddHangfireServer();

        return services;

    }

}
