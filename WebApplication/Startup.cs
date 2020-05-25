using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApplication.DAL;
using WebApplication.Midllewares;
using WebApplication.Services;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer=true,
                        ValidateAudience=true,
                        ValidateLifetime=true,
                        ValidIssuer="Gakko",
                        ValidAudience="Students",
                        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });

            services.AddSingleton<IDbService, MockDbService>();
            services.AddSingleton<IStudentsDbService, SqlServerDbService>();
            // services.AddSingleton<IAccessGuardService, SqlServerAccessGuardService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // app.UseMiddleware<LoggingMiddleware>();
            
            // app.Use(async (context, next) =>
            // {
            //     if (!context.Request.Headers.ContainsKey("Index"))
            //     {
            //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //         await context.Response.WriteAsync("Brak headera Index");
            //         return;
            //     }
            //
            //     string index = context.Request.Headers["Index"].ToString();
            //     
            //     var service = (IAccessGuardService)app.ApplicationServices.GetService(typeof(IAccessGuardService));
            //
            //     if (!service.CanAccess(index))
            //     {
            //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //         await context.Response.WriteAsync("Brak studenta o indexie: " + index);
            //         return;
            //     }
            //
            //     await next();
            // });

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}