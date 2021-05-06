using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Wolf.MessageQueue;
using Wolf.MessageQueue.Services;
using Wolf.Notification.Config;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Middlewares;


namespace Wolf.Notification
{
    public class Startup
    {
        internal const string DbContextConStrName = "NotificationDbContext";

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.WithProperty("Environment", HostingEnvironment.EnvironmentName)
                .CreateLogger();

            var jwtAuthConfigSection = Configuration.GetSection("JwtAuthentication");
            services.Configure<RequestResponseLoggerOptions>(Configuration.GetSection("RequestResponseLogger"))
                .Configure<QueueOptions>(Configuration.GetSection("QueueOptions"))
                .Configure<JwtAuthenticationOptions>(jwtAuthConfigSection)

                .AddDbContext<NotifDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("NotificationDbContext")))
                .AddTransient<IQueueService, RedisMessageQueue>()
                .AddAutoMapper(typeof(Startup));


            var jwtConfig = jwtAuthConfigSection.Get<JwtAuthenticationOptions>();

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerGen();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = jwtConfig.Authority;
                options.RequireHttpsMetadata = jwtConfig.RequireHttpsMetadata;
                options.Audience = jwtConfig.Audience;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("NotificationManagerRoleOrTrustedEnvAndClient", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("aud", jwtConfig.Audience);
                    policy.RequireAssertion(ctx =>
                    {
                        IEnumerable<Claim> claims = ctx.User.Claims;

                        if (claims.Any(c => c.Type == ClaimTypes.Role && c.Value== "NotificationManager")) return true;

                        if (claims.Any(c => c.Type == "client_env" && c.Value == jwtConfig.TrustedClientEnvironment)
                            && claims.Any(c=>c.Type=="client_id" && jwtConfig.TrustedClientIds.Contains(c.Value))) return true;

                        return false;
                    });
                });
            });
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IOptions<RequestResponseLoggerOptions> reqLogOptions, IOptions<JwtAuthenticationOptions> jwtAuthOptions)
        {
            logger.LogInformation($"Current Environment: {env.EnvironmentName}; GetCurrentDirectory: {Directory.GetCurrentDirectory()}");
            var conStr = GetConnectionStringWithEnvPwd(Configuration, DbContextConStrName);
            if (string.IsNullOrEmpty(conStr))
            {
                logger.LogWarning($"Couldn't get a connections string for {DbContextConStrName}");
            }
            else
            {
                var conStrBuilder = new SqlConnectionStringBuilder(conStr);
                logger.LogInformation($"{DbContextConStrName} is set to connect to [{conStrBuilder.DataSource}/{conStrBuilder.InitialCatalog}] with user [{conStrBuilder.UserID}] and InegratedSecurity set to {conStrBuilder.IntegratedSecurity} and passwprd length of {conStrBuilder.Password?.Length}");
            }

            //app.UseSerilogRequestLogging();
            if (reqLogOptions.Value.ShouldEnable)
            {
                app.UseMiddleware<RequestLoggingMiddleware>();
            }

            var jwtSettings = jwtAuthOptions.Value;
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{ConfigureSwaggerGenOptions.GetApiVersion()}/swagger.json", "Notification API");
                c.OAuthClientId(jwtSettings.ClientId);
                c.OAuthClientSecret(jwtSettings.ClientSecret);
                //c.OAuthAppName("Weather API");
                c.OAuthScopeSeparator(" ");
                c.OAuthUsePkce();
                c.RoutePrefix = "";
            });
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }

        internal static string GetConnectionStringWithEnvPwd(IConfiguration config, string conStrName)
        {
            //string dbPwd = config["wolf_notif_svc_db_pwd"];
            var connectionStringBuilder = new SqlConnectionStringBuilder(config.GetConnectionString(conStrName));
            //if (!string.IsNullOrEmpty(dbPwd)) connectionStringBuilder.Password = dbPwd;
            return connectionStringBuilder.ConnectionString;
        }
    }
}
