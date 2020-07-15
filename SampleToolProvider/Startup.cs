using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using MoodleLti;

namespace SampleToolProvider
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddOptions<MoodleLti.Options.MoodleLtiOptions>().Bind(Configuration.GetSection(nameof(MoodleLti.Options.MoodleLtiOptions)));

            // Cookie-based authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = new PathString("/auth");
            });

            // Http client objects
            services.AddTransient<HttpMessageHandlerBuilder, Utilities.CustomHttpMessageHandlerBuilder>();
            services.AddHttpClient( );

            // Moodle LTI service
            services.AddMoodleLtiApi();
            services.AddCachedMoodleGradebook();

            // MVC
            services.AddControllersWithViews(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Debugging
            if(env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Serve static files
            app.UseStaticFiles();

            // Enable routing
            app.UseRouting();

            // Enable access control
            app.UseAuthentication();
            app.UseAuthorization();

            // Other middleware here...

            // Define endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
