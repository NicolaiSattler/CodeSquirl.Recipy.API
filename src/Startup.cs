﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using CodeSquirrel.RecipeApp.Model;
using CodeSquirrel.RecipeApp.DataProvider;
using CodeSquirrel.RecipeApp.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Swashbuckle.AspNetCore.Swagger;
using Newtonsoft.Json.Serialization;

namespace CodeSquirrel.RecipeApp.API
{
    public class Startup
    {
        private const string LOCAL_CLIENT = "http://localhost:4200";

        public IContainer Container { get; private set; }
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory.Split(@"bin\", StringSplitOptions.None);
            var projectDir = path.Length > 0 ? path[0] : string.Empty;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "CodeSquirl - Recipe App",
                    Description = "",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "",
                        Email = "",
                        Url = ""
                    }
                });
            });
        }
        private IContainer ConfigureAutofac(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var builder = new ContainerBuilder();
           
            builder.RegisterModule<ModelModule>();
            builder.RegisterModule<DataProviderModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule(new APIModule(connectionString));
            builder.Populate(services);

            Container = builder.Build();
            return Container;
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .AddJsonOptions(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); })
                    .AddControllersAsServices();

            services.AddCors(options =>
                options.AddPolicy("AllowSpecificOrigin", builder => {
                    builder.WithOrigins(LOCAL_CLIENT);
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                }) 
            );

        #if DEBUG
            ConfigureSwagger(services);
        #endif 

            var container = ConfigureAutofac(services);
            return new AutofacServiceProvider(container);
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseCors("AllowSpecificOrigin");

        #if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                //If localhost:port/swagger/index.html does not work.
                //remove the .vs folder.
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeSquirl - Recipe App (Alpha)");
            });
        #endif
        }
    }
}
