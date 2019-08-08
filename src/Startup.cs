using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddAutoMapper();

            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 1);
                opt.ReportApiVersions = true;
                //opt.ApiVersionReader = new QueryStringApiVersionReader("version"); // Get Version only by Query String
                //opt.ApiVersionReader = new HeaderApiVersionReader("X-Version"); // Get Version only by Request Header
                //opt.ApiVersionReader = new UrlSegmentApiVersionReader(); // Get Version only by URL Request. Note: The Controller has to be changed as well

                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("v", "ver", "version") //Accepts v, ver and version query string to define the version
                ); // Combining to have more than one option to set the version

                // Versioning Conventions
                opt.Conventions.Controller<TalksController>()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .HasApiVersion(new ApiVersion(1, 1))
                    .Action(c => c.Delete(default(string), default(int)))
                    .MapToApiVersion(1, 1);


            });

            services.AddMvc(opt => opt.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
