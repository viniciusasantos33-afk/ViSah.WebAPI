using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ViSah.OpenApi;

public sealed class JwtBearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeName = "BearerAuth";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Cole o JWT retornado por POST /api/auth/login. Não inclua o prefixo 'Bearer '."
        };

        return Task.CompletedTask;
    }
}

public sealed class JwtBearerOperationSecurityTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuthentication = endpointMetadata.OfType<IAuthorizeData>().Any();
        var allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();

        if (!requiresAuthentication || allowsAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtBearerSecuritySchemeTransformer.SchemeName, context.Document)] = []
        });

        return Task.CompletedTask;
    }
}
