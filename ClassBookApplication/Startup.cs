using ClassBookApplication.ActionFilter;
using ClassBookApplication.DataContext;
using ClassBookApplication.Factory;
using ClassBookApplication.Infrastructure;
using ClassBookApplication.Models;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net;
using System.Text;

namespace ClassBookApplication
{
    public class Startup
    {
        #region Fields
        public IConfiguration Configuration { get; }

        #endregion

        #region Ctor
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Method

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            #region Session Management

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            #endregion

            #region Configuration Management

            services.AddControllersWithViews();
            // Add the whole configuration object here.
            services.AddSingleton<IConfiguration>(Configuration);

            #endregion

            #region Inject Database

            services.AddDbContext<ClassBookManagementContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClassBookManagementeDatabase")));

            services.AddDbContext<ClassBookLogsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClassBookManagementeDatabase")));

            services.AddDbContext<ChannelPartnerManagementContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ChannelPartnerManagementeDatabase")));

            #endregion

            #region Inject Filters
            services.AddScoped<ControllerFilterExample>();
            #endregion

            #region Inject Services

            services.AddTransient<FileService, FileService>();
            services.AddTransient<ClassBookService, ClassBookService>();
            services.AddTransient<NotificationService, NotificationService>();
            services.AddTransient<LogsService, LogsService>();
            services.AddTransient<MyAuthencationService, MyAuthencationService>();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ClassBookModelFactory, ClassBookModelFactory>();

            services.AddScoped<IViewRenderService, ViewRenderService>();
            #endregion

            #region Version Management

            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });

            #endregion

            #region Compatibality Configuration
            // Compatiblity version set to Dot Net Core 3.0
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            #endregion

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(name: "v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "ClassBookAPI", Version = "v1" });
                c.OperationFilter<CustomHeaderSwaggerAttribute>();
            });

            #endregion

            #region Authencation

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"]))
                };
            });
            IdentityModelEventSource.ShowPII = true;

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.Admin, Policies.AdminPolicy());
                config.AddPolicy(Policies.User, Policies.UserPolicy());
            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            #region Exception Handling for WebApplication and API

            app.UseWhen(context => context.Request.Path.Value.Contains("/api"), appBuilder =>
            {
                appBuilder.UseMiddleware<ExceptionMiddleware>();
            });
            app.UseWhen(context => !context.Request.Path.Value.Contains("/api"), appBuilder =>
            {
                appBuilder.UseMiddleware<WebsiteExceptionMiddleware>();
            });

            #endregion

            #region Authorization and Authencation

            app.UseStaticFiles();
            app.UseCookiePolicy();
            ////Add User session
            app.UseSession();
            //Add JWToken to all incoming HTTP Request Header
            app.Use(async (context, next) =>
            {
                var JWToken = context.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(JWToken))
                {
                    context.Request.Headers.Add("Authorization", JWToken);
                }
                await next();
            });
            app.UseStatusCodePages(async context =>
            {
                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;
                var path = request.Path.Value ?? "";

                if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    response.Redirect("/User/Login");
                }
            });
            //Add JWToken Authentication service
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            #endregion

            #region Routing Map

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            #endregion

            #region Swagger settings

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "ClassBookAPI v1");
            });

            #endregion
        }

        #endregion
    }
}