using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OdeToFood.Data;
using OdeToFood.Services;

namespace OdeToFood
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            // In here we can access pre-registered services before the call to ConfigureServices
            _configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGreeter, Greeter>();

            services.AddDbContext<OdeToFoodDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("OdeToFood")));
            
            services.AddScoped<IRestaurantData, SqlRestaurantData>();
            
            // Add MVC services to route requests through the MVC framework
            services.AddMvc(options =>
            {
                options.SslPort = 44321;
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // Set the anti-forgery token cookie to use a secure cookie
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "_af";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.HeaderName = "X-XSRF-TOKEN";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            // Always use SSL (HTTPS)
            app.UseRewriter(new RewriteOptions().AddRedirectToHttpsPermanent());

            // serve a file from wwwroot only if URL matches exactly
            app.UseStaticFiles();

            app.UseMvc(ConfigureRoutes);

            app.Run(async (context) =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Not found");
            });
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Default", 
                "{controller=Home}/{action=Index}/{id?}");
        }
    }
}