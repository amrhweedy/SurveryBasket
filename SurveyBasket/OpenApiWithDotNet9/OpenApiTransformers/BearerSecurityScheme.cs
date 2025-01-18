using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OpenApiWithDotNet9.OpenApiTransformers;

//Its purpose is to dynamically add a Bearer security scheme to the OpenAPI document if such an authentication scheme is configured in the application.

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }

        }
    }
}


//The provided class, `BearerSecurityScheme`, is a custom implementation of an **OpenAPI document transformer** for a .NET 9 application using OpenAPI. Its purpose is to dynamically add a `Bearer` security scheme to the OpenAPI document if such an authentication scheme is configured in the application.

//Here's a detailed breakdown of the class:

//---

//Key Components

//2. Constructor**:
//   - The constructor accepts an `IAuthenticationSchemeProvider` instance, which allows the class to query the authentication schemes configured in the application.

//3. Interface Implementation**:
//   - Implements `IOpenApiDocumentTransformer`, which provides a `TransformAsync` method used to modify the OpenAPI document before it is served.

//4.`TransformAsync` Method**:
//   - This method takes three parameters:
//     - `OpenApiDocument document`: The OpenAPI document to modify.
//     - `OpenApiDocumentTransformerContext context`: Context for the transformation (not used in this implementation).
//     - `CancellationToken cancellationToken`: Allows cancellation of the operation.
//   - The method performs the transformation asynchronously.

//---

//Functionality**
//1. Check for a `Bearer` Authentication Scheme:
//   - The method calls `authenticationSchemeProvider.GetAllSchemesAsync()` to fetch all registered authentication schemes in the application.
//   - It checks if any scheme has the name `"Bearer"`.

//2. Add a `Bearer` Security Scheme to the OpenAPI Document:
//   - If a `Bearer` authentication scheme is found:
//     - A security scheme is defined using the `OpenApiSecurityScheme` class with the following properties:
//       -  Type: `Http` (indicating HTTP-based authentication).
//       -  Scheme: `"bearer"` (the header type used for the `Authorization` header).
//       -  In: `Header` (indicates where the credential is sent).
//       -  BearerFormat: `"Json Web Token"` (provides additional information about the format).
//     - The scheme is added to the `SecuritySchemes` dictionary of the OpenAPI document's `Components` section.

//3.  Modify the Document:
//   - Ensures `document.Components` is initialized if it's null.
//   - Sets the `SecuritySchemes` dictionary with the newly defined `Bearer` security scheme.

//---

// Purpose and Use Case
//This class enables the integration of a `Bearer` authentication scheme into the OpenAPI document dynamically, ensuring that:
//- The OpenAPI documentation accurately reflects the application's security requirements.
//- Developers or API consumers see the `Bearer` token requirement in the API specification.

//---

//Example Scenario
//1.  Context:
//   - An application uses JWT(JSON Web Token) authentication via a `Bearer` token.
//   - The API documentation must include the `Bearer` token security scheme for clarity and proper API client integration.

//2. Outcome:
//   - When the `BearerSecurityScheme` transformer is invoked during OpenAPI generation:
//     - If a `Bearer` authentication scheme is configured, it updates the OpenAPI document to include the corresponding security scheme.
//     - This makes the OpenAPI documentation compliant with the application's authentication mechanism.

//---

//### How to Use This Class
//1. Register this transformer in your dependency injection container.
//2. Add it to the OpenAPI pipeline so it modifies the document before it is served.

//services.AddSingleton<IOpenApiDocumentTransformer, BearerSecurityScheme>();


//This way, the OpenAPI document automatically reflects the security scheme required by your application.