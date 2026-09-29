using Autofac;
using Autofac.Extensions.DependencyInjection;
using InventoryManagement.Application.Helpers;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Services;
using InventoryManagement.Infrastructure.UnitOfWork;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ModelBinders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;

/// <summary>
/// Main application entry point.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}");
        });

        // Configure built-in services
        ConfigureBuiltInServices(builder.Services, builder.Configuration);

        // Configure Autofac (Third-party DI container)
        // Autofac will replace built-in DI container for the services registered in Autofac
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            ConfigureAutofac(containerBuilder);
        });

        var app = builder.Build();

        // Configure pipeline
        ConfigurePipeline(app);

        // Initialize database
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.Run();
    }

    /// <summary>
    /// Configures the built-in Microsoft DI services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration instance.</param>
    private static void ConfigureBuiltInServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Add database context
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ============================================================
        // Services Registration
        // ============================================================
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // ============================================================
        // FILTERS - ServiceFilter Registration
        // ============================================================
        services.AddScoped<LoggingActionFilter>();
        services.AddScoped<ValidationActionFilter>();

        // ============================================================
        // REPOSITORIES - Registered in Built-in DI
        // (Autofac will re-register this as well, even so we keep 
        // it registered here to show the capability of using built-in DI)
        // ============================================================
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // ============================================================
        // UNIT OF WORK - Registered in Built-in DI
        // ============================================================
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ============================================================
        // SERVICES - Registered in Built-in DI
        // ============================================================
        services.AddScoped<IAuthService, AuthService>();

        // Add MVC with custom view engines
        services.AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                // Default Razor view engine configuration
                options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        // Add authentication (JWT)
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSettings?.SecretKey 
                    ?? throw new ArgumentNullException("SecretKey")))
            };

            //Read access token from cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Read access token from cookie
                    context.Token = context.Request.Cookies["AccessToken"];
                    return Task.CompletedTask;
                }
            };
        });

        // Add authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("UserOrAdmin", policy =>
                policy.RequireRole("User", "Admin"));
        });

        // Add session configuration
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Add cookie Configuration
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => false;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.Secure = CookieSecurePolicy.Always;
        });

        // Add HTTP context accessor (for AuthController)
        services.AddHttpContextAccessor();

        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        // Add MVC Areas
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
            options.AppendTrailingSlash = true;
        });
    }

    /// <summary>
    /// Configures Autofac container.
    /// </summary>
    /// <param name="builder">Autofac container builder.</param>
    private static void ConfigureAutofac(ContainerBuilder builder)
    {
        // ============================================================
        // AUTOFAC DI DEMO - Third-party DI Container
        // ============================================================

        // Register repositories dengan Autofac
        builder.RegisterType<ProductRepository>()
            .As<IProductRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<WarehouseRepository>()
            .As<IWarehouseRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TransactionRepository>()
            .As<ITransactionRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CategoryRepository>()
            .As<ICategoryRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<UserRepository>()
            .As<IUserRepository>()
            .InstancePerLifetimeScope();

        // Register Unit of Work with Autofac
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        // Register Services with Autofac
        builder.RegisterType<AuthService>()
            .As<IAuthService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ProductService>()
            .As<IProductService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<WarehouseService>()
            .As<IWarehouseService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TransactionService>()
            .As<ITransactionService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CategoryService>()
            .As<ICategoryService>()
            .InstancePerLifetimeScope();

        // Register filters with Autofac
        builder.RegisterType<LoggingActionFilter>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ValidationActionFilter>()
            .InstancePerLifetimeScope();

        // Register DbContext with Autofac
        // (This will use the DbContext that is already registered in the built-in DI)
        builder.RegisterType<AppDbContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

        // Register View Engine with Autofac
        builder.RegisterType<RazorViewEngine>()
            .AsSelf()
            .InstancePerLifetimeScope();

        // Inject ILogger with Autofac
        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .InstancePerLifetimeScope();
    }

    /// <summary>
    /// Configures the application pipeline.
    /// </summary>
    /// <param name="app">Application builder.</param>
    private static void ConfigurePipeline(WebApplication app)
    {
        // Configure logging
        app.UseSerilogRequestLogging();

        // Configure error handling
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Configure security
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Configure routing
        app.UseRouting();

        // Configure authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure session
        app.UseSession();

        // Configure area routing
        app.MapAreaControllerRoute(
            name: "areas",
            areaName: "Auth",
            pattern: "Auth/{controller=Auth}/{action=Login}/{id?}");

        // Configure default routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Configure endpoint routing
        app.MapControllerRoute(
            name: "api",
            pattern: "api/{controller}/{action}/{id?}");

        // Configure health checks
        app.MapHealthChecks("/health");

        // Configure fallback for SPA-like behavior
        app.MapFallbackToController("Index", "Home");
    }
}
