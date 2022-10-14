using System;
using System.IO;
using System.Reflection;
using Arrba.Parser.Api.AttributeFilters;
using Arrba.Parser.DbContext;
using Arrba.Parser.JobsSave;
using Arrba.Parser.JobsUrl;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Mapper;
using Arrba.Parser.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Arrba.Parser.Provider.Realization;

namespace Arrba.Parser.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(SwaggerConfiguration);
            services.AddHangfire(config => config.UsePostgreSqlStorage(Configuration.GetConnectionString("HangfireConnection")));

            services.AddDbContext<ParserDbContext>();
            services.AddTransient<ILogService, LogService>();
            services.AddSingleton<DataDictionaries>();
            services.AddTransient<BaseMapper, DictionaryMapper>();

            services.AddTransient<UrlManager>();
            services.AddTransient<SaveManager>();
            services.AddTransient<DeactualizeManager>();           
            services.AddTransient(o => new MagazinpricepovRuProvider(new HttpBaseClient()));
            services.AddTransient(o => new RolfProbegRuProvider(new HttpBaseClient()));
            services.AddTransient(o => new Pricep36RfProvider(new HttpBaseClient()));

            var provider = services.BuildServiceProvider();
            var logService = provider.GetService<ILogService>();
            var context = provider.GetService<ParserDbContext>();
            var mapper = provider.GetService<BaseMapper>();


            var voronezhRuProvider = provider.GetService<PricepiVoronezhRuProvider>();
            services.AddScoped(o => new PricepiVoronezhRuUrlJob(voronezhRuProvider, logService, context));
            services.AddScoped(o => new PricepiVoronezhRuSaveJob(voronezhRuProvider, logService, mapper, context));

            var rolfRuProvider = provider.GetService<RolfProbegRuProvider>();
            services.AddScoped(o => new RolfProbegRuUrlJob(rolfRuProvider, logService, context));
            services.AddScoped(o => new RolfProbegRuSaveJob(rolfRuProvider, logService, mapper, context));


            var pricep36RfProvider = provider.GetService<Pricep36RfProvider>();
            services.AddScoped(o => new Pricep36RfUrlJob(pricep36RfProvider, logService, context));
            services.AddScoped(o => new Pricep36RfSaveJob(pricep36RfProvider, logService, mapper, context));


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IServiceProvider serviceProvider,
            IHostingEnvironment env,
            ILogService logService,
            BaseMapper mapper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Arrba Parser API v1");
                //To serve the Swagger UI at the app's root (http://localhost:<port>/), set the RoutePrefix property to an empty 
                c.RoutePrefix = string.Empty;
            });

            // Fo Handfire DI
            // https://codeburst.io/schedule-background-jobs-using-hangfire-in-net-core-2d98eb64b196
            // GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            app.UseHangfireServer();
            app.UseHangfireDashboard($"/hf", options: new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangFireAuthorization(
                        app.ApplicationServices.GetService<IAuthorizationService>(),
                        app.ApplicationServices.GetService<IHttpContextAccessor>())
                },
                // IsReadOnlyFunc = context => true
            });
            app.UseHttpsRedirection();
            app.UseMvcWithDefaultRoute();


            //var magazinpricepovRuSaveJob = serviceProvider.GetService<MagazinpricepovRuSaveJob>();
            //var magazinpricepovRuUrlJob = serviceProvider.GetService<MagazinpricepovRuUrlJob>();

            //RecurringJob.AddOrUpdate("https://magazinpricepov.ru url", () => magazinpricepovRuUrlJob.Run(), "0 */2 * 12 *");
            //RecurringJob.AddOrUpdate("https://magazinpricepov.ru save", () => magazinpricepovRuSaveJob.Run(), "0 */2 * 12 *");

            var pricepiVoronezhRuUrlJob = serviceProvider.GetService<PricepiVoronezhRuUrlJob>();
            var ricepiVoronezhRuSaveJob = serviceProvider.GetService<PricepiVoronezhRuSaveJob>();
            RecurringJob.AddOrUpdate("http://прицеп-воронеж.рф url", () => pricepiVoronezhRuUrlJob.Run(), "0 */2 * 12 *");
            RecurringJob.AddOrUpdate("http://прицеп-воронеж.рф save", () => ricepiVoronezhRuSaveJob.Run(), "0 */2 * 12 *");


            var rolfProbegRuUrlJob = serviceProvider.GetService<RolfProbegRuUrlJob>();
            var rolfProbegRuSaveJob = serviceProvider.GetService<RolfProbegRuSaveJob>();
            RecurringJob.AddOrUpdate("https://rolfProbeg.ru url", () => rolfProbegRuUrlJob.Run(), "0 */2 * 12 *");
            RecurringJob.AddOrUpdate("https://rolfProbeg.ru save", () => rolfProbegRuSaveJob.Run(), "0 */2 * 12 *");


            var pricep36RfUrlJob = serviceProvider.GetService<Pricep36RfUrlJob>();
            var pricep36RfSaveJob = serviceProvider.GetService<Pricep36RfSaveJob>();
            RecurringJob.AddOrUpdate("http://прицеп36.рф url", () => pricep36RfUrlJob.Run(), "0 */2 * 12 *");
            RecurringJob.AddOrUpdate("http://прицеп36.рф save", () => pricep36RfSaveJob.Run(), "0 */2 * 12 *");

        }

        void SwaggerConfiguration(SwaggerGenOptions c)
        {
            c.SwaggerDoc("v1", new Info { Title = "Arrba Parser API", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        }

        //public class HangfireActivator : JobActivator
        //{
        //    private readonly IServiceProvider _serviceProvider;

        //    public HangfireActivator(IServiceProvider serviceProvider)
        //    {
        //        _serviceProvider = serviceProvider;
        //    }

        //    public override object ActivateJob(Type type)
        //    {
        //        return _serviceProvider.GetService(type);
        //    }
        //}

    }
}
