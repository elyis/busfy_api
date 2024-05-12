using System.Text;
using busfy_api.src.App.IService;
using busfy_api.src.App.Service;
using busfy_api.src.Domain.Entities.Config;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using busfy_api.src.Shared.Filters;
using busfy_api.src.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MimeDetective;
using MimeDetective.Definitions.Licensing;
using Swashbuckle.AspNetCore.Filters;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Provider;
using webApiTemplate.src.App.Service;
using webApiTemplate.src.Domain.Entities.Config;

namespace busfy_api
{

    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        private void ConfigureConfigServise(IServiceCollection services)
        {
            var emailServiceSettings = _config.GetSection(nameof(EmailServiceSettings)).Get<EmailServiceSettings>() ?? throw new Exception("email service settings is empty");
            var locationServiceSettings = _config.GetSection("LocationServiceSettings").Get<LocationServiceSettings>() ?? throw new Exception("location service settings is empty");


            var fileInspector = new ContentInspectorBuilder()
            {
                Definitions = new MimeDetective.Definitions.CondensedBuilder()
                {
                    UsageType = UsageType.PersonalNonCommercial
                }.Build()
            }.Build();



            services.AddSingleton(fileInspector);
            services.AddSingleton(emailServiceSettings);
            services.AddSingleton(locationServiceSettings);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureConfigServise(services);
            var jwtSettings = _config.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new Exception("jwt settings is empty");
            var redisSettings = _config.GetSection("RedisSettings").Get<RedisSettings>() ?? throw new Exception("redis settings is empty");

            services.AddControllers(config =>
            {
                config.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            })
            .AddNewtonsoftJson()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
            });

            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(options =>
                {
                    options.AllowAnyHeader();
                    options.AllowAnyOrigin();
                    options.AllowAnyMethod();
                });
            });
            services.AddEndpointsApiExplorer();
            services.AddDbContext<AppDbContext>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = redisSettings.InstanceName;
            });

            services
                .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                });

            services.AddAuthorization();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "busfy_api",
                    Description = "Api",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Bearer auth scheme",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.OperationFilter<DefaultSwaggerOperationFilter>();

                options.EnableAnnotations();
            });
            services.AddSwaggerGenNewtonsoftSupport();



            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IFileUploaderService, LocalFileUploaderService>();
            services.AddSingleton<ILocationService, LocationService>();
            services.AddSingleton(jwtSettings);

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Repository")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Manager")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Service")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseHttpLogging();
            app.UseRequestLocalization();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Resources")),
                RequestPath = "/Resources"
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            // app.UseMiddleware<CustomStatusCodeMiddleware>();

            app.MapControllers();
            InitDatabase(_config);
            app.Run();
        }

        private void InitDatabase(IConfiguration config)
        {
            var context = new AppDbContext(new DbContextOptions<AppDbContext>(), config);
            var email = "admin@gmail.com";

            var user = context.Users.FirstOrDefault(e => e.Email == email);
            if (user == null)
            {
                user = new UserModel
                {
                    Email = email,
                    AccountStatus = UserAccountStatus.Active.ToString(),
                    Nickname = "Дуров верни стену",
                    RoleName = UserRole.Admin.ToString(),
                    UserTag = "ton-crypto",
                    PasswordHash = Hmac512Provider.Compute("roottoor")
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
    }
}