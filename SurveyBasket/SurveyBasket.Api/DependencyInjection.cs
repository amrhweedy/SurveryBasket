using Asp.Versioning;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Health;
using SurveyBasket.Api.Services.Authentication;
using SurveyBasket.Api.Services.BackgroundJobs;
using SurveyBasket.Api.Services.Cashing;
using SurveyBasket.Api.Services.Emails;
using SurveyBasket.Api.Services.Results;
using SurveyBasket.Api.Services.Roles;
using SurveyBasket.Api.Services.Users;
using SurveyBasket.Api.Services.Votes;
using SurveyBasket.Api.Settings;
using SurveyBasket.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Threading.RateLimiting;

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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();


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

        services.AddOptions<MailSettings>()
            .BindConfiguration(nameof(MailSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();



        // add health check service
        services.AddHealthChecks()
            .AddSqlServer(name: "Database", connectionString: connectionString)
            .AddHangfire(options => { options.MinimumAvailableServers = 1; })  // we need a at least 1 server the hanfire works to it to the health check is healthy
            .AddUrlGroup(name: "google api", uri: new Uri("https://www.google.com"), tags: ["api"], httpMethod: HttpMethod.Get) // if we call this url from my api and i need to check if the url is correct and the method is get, so if this correct is correct the health check is healthy
            .AddUrlGroup(name: "facebook api", uri: new Uri("https://www.facebook.com"), tags: ["api"]) // we can add tags to group some health checks with each other 
            .AddCheck<MailProviderHealthCheck>(name: "mail provider");


        #region rate limiter

        // concurrency rate limiting

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // the default rejection status code is 503 (Service Unavailable). so we change it to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddConcurrencyLimiter("concurrency", concurrencyLimiterOptions =>
          {
              concurrencyLimiterOptions.PermitLimit = 1000;  //maximum number of requests that can be processed simultaneously (at the same time).

              //maximum number of requests that can be queued (wait in line) while waiting for one of the currently running requests to finish.
              //In this case, if more than 2 requests come in at once, only 1 additional request can wait in the queue.
              // If more than 1 request is waiting, the server will reject the extra requests(returning HTTP 429 status code).
              concurrencyLimiterOptions.QueueLimit = 100;


              concurrencyLimiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; //the oldest request in the queue will be processed first)
          });
        }

       );



        // Token Bucket Limiter

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // the default rejection status code is 503 (Service Unavailable). so we change it to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddTokenBucketLimiter("tokenBucketLimiter", tokenLimiterOptions =>
            {
                tokenLimiterOptions.TokenLimit = 2;  //maximum number of tokens that can be existed in the bucket

                // maximum number of requests that can be queued (wait in line) while waiting until the bucket has more tokens.
                // In this case, if the token limit is 2 and ther are more 4 requests come at the same time, the firt 2 requests take the 2 tokens that in the bucket and the buecket will be empty of tokens after these 2 requests.
                // the third request will wait int the queue until there are tokens again in the bucket
                // the fourth request will be rejected(returning HTTP 429 status code). because the max size of the queue is 1 and the max size of the bucket is 2
                tokenLimiterOptions.QueueLimit = 1;
                tokenLimiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; //the oldest request in the queue will be processed first

                tokenLimiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(30); // the bucket will be refilled every 30 seconds until the tokens in the bucket are reached to the max tokenlimit 2
                tokenLimiterOptions.TokensPerPeriod = 1; // the bucket will be refilled with 1 tokens every 30 seconds unitl the tokens in the bucket are reached to the max tokenlimit 2
                tokenLimiterOptions.AutoReplenishment = true;  // the bucket will be refilled automatically when the number of tokens in the bucked are less than the max token limit

            });
        }

       );



        // Fixed Window Limiter

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // the default rejection status code is 503 (Service Unavailable). so we change it to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddFixedWindowLimiter("fixedWindowLimiter", fixedWindowlLimiterOptions =>
            {
                fixedWindowlLimiterOptions.PermitLimit = 2;  //maximum number of requests that can be processed or recieved throught 3 
                fixedWindowlLimiterOptions.Window = TimeSpan.FromSeconds(20); // it means we can handle or recieve 2 requests in 20 seconds

                // maximum number of requests that can be queued (wait in line) until the window time is finished, unitl the 20 seconds is over then the request which is in the queue will be processed
                // In this case, if the PermitLimit is 2 and ther are more 4 requests come at the same time, the firt 2 requests will be processed
                // the third request will wait int the queue until the window time is over , until the 20 seconds is over
                // the fourth request will be rejected(returning HTTP 429 status code). because the max size of the queue is 1 and the max size of the permit limit is 2
                fixedWindowlLimiterOptions.QueueLimit = 1;
                fixedWindowlLimiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; //the oldest request in the queue will be processed first

            });
        }

       );


        // sliding Window Limiter ( listen again)

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // the default rejection status code is 503 (Service Unavailable). so we change it to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
            {
                options.PermitLimit = 2;
                options.Window = TimeSpan.FromSeconds(20);
                options.SegmentsPerWindow = 2;
                options.QueueLimit = 1;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

            });
        }

       );

        // IP Address Limiter => is a way to control the number of requests allowed from a specific IP address within a certain period of time. It’s a type of rate limiting that applies limits based on the client's IP address.
        services.AddRateLimiter(rateLimiterOptions =>
        {
            // Change the rejection status code to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy("ipLimit", httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2, // Maximum number of requests allowed from a specific IP address is 2 through 20 seconds if he sends more than 2 requests in 20 seconds it will be rejected (returning HTTP 429 status code (too many requests))
                        Window = TimeSpan.FromSeconds(20)// it means we can handle or recieve 2 requests in 20 seconds from the same ip address
                    }
                );
            });
        });



        // user limiter => is a system that limits how many actions or requests a specific user can make within a certain period of time.
        // It ensures that each user gets fair access to the system and prevents overloading or abuse by individual users.
        services.AddRateLimiter(rateLimiterOptions =>
        {
            // Change the rejection status code to 429 (Too Many Requests)
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy("userLimit", httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.GetUserId(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2, // Maximum number of requests allowed from a specific user is 2 through 20 seconds if he sends more than 2 requests in 20 seconds it will be rejected (returning HTTP 429 status code (too many requests))
                        Window = TimeSpan.FromSeconds(20)// it means we can handle or recieve 2 requests in 20 seconds from the same ip address
                    }
                );
            });
        });

        #endregion


        #region API Versioning
        // api versioning
        //1- UrlSegmentApiVersionReader

        //services.AddApiVersioning(options =>
        //{
        //    options.DefaultApiVersion = new ApiVersion(1.0);
        //    options.AssumeDefaultVersionWhenUnspecified = true;
        //    options.ReportApiVersions = true;   // tell the client which api version is the current in the response header 

        //    options.ApiVersionReader = new UrlSegmentApiVersionReader();

        //}).AddApiExplorer(options =>
        //{
        //    options.GroupNameFormat = "'v'V";
        //    options.SubstituteApiVersionInUrl = true;
        //});

        // 2-HeaderApiVersionReader

        services.AddApiVersioning(options =>
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



        //3 - we can use more than one api version readers

        //services.AddApiVersioning(options =>
        //{
        //    options.DefaultApiVersion = new ApiVersion(1);
        //    options.AssumeDefaultVersionWhenUnspecified = true;

        //    options.ApiVersionReader = ApiVersionReader.Combine(
        //        new UrlSegmentApiVersionReader(),
        //        new HeaderApiVersionReader("x-api-version");

        //}).AddApiExplorer(options =>
        //{
        //    options.GroupNameFormat = "'v'V";
        //    options.SubstituteApiVersionInUrl = true;
        //});

        #endregion


        return services;
    }

    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {

            // we dont need this configuration because we make dynamic class(ConfigureSwaggerOptions) scan all versions 
            // we make swagger support the Authorization in this file (ConfigureSwaggerOptions) also
            #region swagger configuration manually
            //options.SwaggerDoc("v1", new OpenApiInfo
            //{
            //    Version = "v1",
            //    Title = "SurveyBasket.Api",
            //    Description = 
            //    TermsOfService = new Uri("https://example.com/terms"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "Example Contact",
            //        Url = new Uri("https://example.com/contact")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://example.com/license")
            //    }
            //});

            //options.SwaggerDoc("v2", new OpenApiInfo
            //{
            //    Version = "v2",
            //    Title = "SurveyBasket.Api",
            //    Description = "version 2",
            //    TermsOfService = new Uri("https://example.com/terms"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "Example Contact",
            //        Url = new Uri("https://example.com/contact")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://example.com/license")
            //    }
            //});

            #endregion


            // display the xml comments on the endpoints in the swagger ui
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            // take the number of api versions from the swagger ui and set it automatically in header x-api-version
            options.OperationFilter<SwaggerDefaultValues>();
        });

        // for enable the api versioning in swagger
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

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

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();


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

            // Default Lockout settings we dont need to change it. to enbale the lock we need to make lockoutOnFailure = true when using the _signInManager.PasswordSignInAsync method in auth service

            // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            // options.Lockout.MaxFailedAccessAttempts = 5;
            // options.Lockout.AllowedForNewUsers = true;
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
