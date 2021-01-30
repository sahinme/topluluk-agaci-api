using Ardalis.ListStartupServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.Web.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Nnn.ApplicationCore.Entities.EmailSettings;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services;
using Microsoft.Nnn.ApplicationCore.Services.BlobService;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService;
using Microsoft.Nnn.ApplicationCore.Services.CommentService;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService;
using Microsoft.Nnn.ApplicationCore.Services.MessageService;
using Microsoft.Nnn.ApplicationCore.Services.NotificationService;
using Microsoft.Nnn.ApplicationCore.Services.PostService;
using Microsoft.Nnn.ApplicationCore.Services.RealNotify;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService;
using Microsoft.Nnn.ApplicationCore.Services.SuggestionService;
using Microsoft.Nnn.ApplicationCore.Services.UserService;
using Microsoft.Nnn.Infrastructure.Data;
using Microsoft.Nnn.Infrastructure.Logging;
using Microsoft.Nnn.Infrastructure.Services;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Nnn.Web
{
    public class Startup
    {
        private IServiceCollection _services;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
         
            ConfigureProductionServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            
            services.AddDbContext<NnnContext>(c =>
                c.UseSqlServer(Configuration.GetConnectionString("Nnn")),ServiceLifetime.Transient);

            ConfigureServices(services);
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureCookieSettings(services);

            IdentityModelEventSource.ShowPII = true;

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:3000/","http://saalla.com/","https://saalla.com/");
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
                builder.SetIsOriginAllowed((host) => true);
                builder.AllowCredentials();
            }));
            
            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

            services.AddScoped<IPostAppService, PostAppService>();
            services.AddScoped<ICommentAppService, CommentAppService>();
            services.AddScoped<IReplyAppService, ReplyAppService>();
            services.AddScoped<ICategoryAppService, CategoryAppService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IConversationAppService, ConversationAppService>();
            services.AddScoped<INotificationAppService, NotificationAppService>();
            services.AddScoped<IMessageAppService, MessageAppService>();
            services.AddScoped<ISuggestionAppService, SuggestionAppService>();
            services.AddScoped<ICommunityAppService, CommunityAppService>();
            services.AddScoped<IBlobService, BlobService>();
            services.Configure<CatalogSettings>(Configuration);
            services.AddSingleton<IUriComposer>(new UriComposer(Configuration.Get<CatalogSettings>()));

            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddTransient<IEmailSender, EmailSender>();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            
            services.AddAutoMapper(typeof(Startup));
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Issuer"],
                        ValidAudience = Configuration["Audience"],
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration.GetValue<string>("SigningKey"))
                        ),
                    };
                });
            
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.PayloadSerializerSettings.Converters.Add(new StringEnumConverter());
            });
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            
            // Add memory cache services
            services.AddMemoryCache();

            services.AddRouting(options =>
            {
                // Replace the type and the name used to refer to it with your own
                // IOutboundParameterTransformer implementation
                options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
            });
            
            services.Configure<MyConfig>(Configuration.GetSection("MyConfig"));
            services.AddMvc(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(
                         new SlugifyParameterTransformer()));
                
            }
            )
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizePage("/Basket/Checkout");
                    options.AllowAreas = true;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpContextAccessor();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "NnnApi", Version = "v1" });
            });

            services.AddHealthChecks()
                .AddCheck<HomePageHealthCheck>("home_page_health_check")
                .AddCheck<ApiHealthCheck>("api_health_check");

            services.Configure<ServiceConfig>(config =>
            {
                config.Services = new List<ServiceDescriptor>(services);

                config.Path = "/allservices";
            });

            _services = services; // used to debug registered services
        }

        private static void ConfigureCookieSettings(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.Cookie = new CookieBuilder
                {
                    IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("MyPolicy");
            
            app.UseSignalR(routes => 
            {
                routes.MapHub<NotificationsHub>("/notifications");
            });
            
            app.UseAuthentication();

            //app.UseDeveloperExceptionPage();
            app.UseHealthChecks("/health",
                new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonConvert.SerializeObject(
                            new
                            {
                                status = report.Status.ToString(),
                                errors = report.Entries.Select(e => new
                                {
                                    key = e.Key,
                                    value = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                                })
                            });
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseShowAllServicesMiddleware();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NnnAPI V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller:slugify=Home}/{action:slugify=Index}/{id?}");
            });
        }
    }
}
