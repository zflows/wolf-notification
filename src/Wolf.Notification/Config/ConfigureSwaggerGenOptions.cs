using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace Wolf.Notification.Config
{
    //adopted from https://lurumad.github.io/swagger-ui-with-pkce-using-swashbuckle-asp-net-core
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly JwtAuthenticationOptions _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConfigureSwaggerGenOptions> _logger;

        public ConfigureSwaggerGenOptions(IOptions<JwtAuthenticationOptions> settings, IHttpClientFactory httpClientFactory, ILogger<ConfigureSwaggerGenOptions> logger)
        {
            _settings = settings.Value;
			_httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public void Configure(SwaggerGenOptions options)
        {
            var currentAssembly = typeof(ConfigureSwaggerGenOptions).Assembly;
            string versionStr = GetApiVersion(currentAssembly);

            var discoveryDocument = GetDiscoveryDocument();
            _logger.LogInformation("Got Auth Discovery document from {authority}, with\r\n\t AuthorizeEndpoint: {AuthorizeEndpoint},\r\n\t TokenEndpoint: {TokenEndpoint}", _settings.Authority, discoveryDocument.AuthorizeEndpoint, discoveryDocument.TokenEndpoint);

            options.OperationFilter<AuthorizeOperationFilter>();
            options.DescribeAllParametersInCamelCase();
            options.CustomSchemaIds(x => x.FullName);
            options.SwaggerDoc(versionStr, CreateOpenApiInfo(versionStr));
            options.UseAllOfToExtendReferenceSchemas();

            // Configure Swagger to use the xml documentation file
            var xmlFile = Path.ChangeExtension(currentAssembly.Location, ".xml");
            options.IncludeXmlComments(xmlFile);

            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                        TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                        Scopes = new Dictionary<string, string>
                        {
                            { _settings.Audience , "Wolf Notification Api Audience" },
                            {"openid", "OpenID"}
                        },
                    }
                },
                Description = "Balea Server OpenId Security Scheme"
            });
        }

        private DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            return _httpClientFactory.CreateClient().GetDiscoveryDocumentAsync(_settings.Authority).GetAwaiter().GetResult();
        }

        private OpenApiInfo CreateOpenApiInfo(string versionStr)
        {
            return new OpenApiInfo()
            {
                Title = "Wolf Notification API",
                Version = versionStr,
                Description = "Wolf API for Managing Notification Templates and Messages and for Message-delivery",
                Contact = new OpenApiContact() { Name = "API", Url = new Uri("https://www.zflows.com/") },
                License = new OpenApiLicense()
            };
        }

        internal static string GetApiVersion(Assembly assembly=null)
		{
            if(null== assembly) assembly=typeof(ConfigureSwaggerGenOptions).Assembly;
            var version = assembly.GetName()?.Version;
            if (null == version) version = new Version(1, 0);
            return $"v{version.Major}.{version.Minor}";
        }
    }
}