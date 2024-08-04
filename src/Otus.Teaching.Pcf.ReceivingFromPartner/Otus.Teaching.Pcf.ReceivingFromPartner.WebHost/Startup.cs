using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Otus.Pcf.RabbitMq;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Notifiers;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.ReceivingFromPartner.DataAccess;
using Otus.Teaching.Pcf.ReceivingFromPartner.DataAccess.Data;
using Otus.Teaching.Pcf.ReceivingFromPartner.DataAccess.Repositories;
using Otus.Teaching.Pcf.ReceivingFromPartner.Integration;
using Otus.Teaching.Pcf.ReceivingFromPartner.Integration.Notifiers;
using System;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Otus.Teaching.Pcf.ReceivingFromPartner.WebHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddMvcOptions(x => x.SuppressAsyncSuffixInActionNames = false);
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbInitializer, EfDbInitializer>();

            services.AddDbContext<DataContext>(x =>
            {
                x.UseNpgsql(Configuration.GetConnectionString("PromocodeFactoryReceivingFromPartnerDb"));
                x.UseSnakeCaseNamingConvention();
                x.UseLazyLoadingProxies();
            });
            services.ConfigureRabbitServices(Configuration);

            services.AddScoped<IAdministrationNotifier, AdministrationNotifier>();
            services.AddScoped<IGivingPromoCodeToCustomerNotifier, GivingPromoCodeToCustomerNotifier>();
            services.AddScoped<INotificationGateway, NotificationGateway>();

            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory Receiving From Partner API Doc";
                options.Version = "1.0";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            dbInitializer.InitializeDb();
        }
    }
}