using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using URF.Core.Abstractions;
using URF.Core.EF;
using Demo.UrfCore3.EF.Models;
using Demo.UrfCore3.EF.UnitsOfWork;

namespace Demo.UrfCore3.Api
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
            services.AddControllers();
            var connectionString = Configuration.GetConnectionString(nameof(UrfDemoContext));
            services.AddDbContext<UrfDemoContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<DbContext, UrfDemoContext>();
            services.AddScoped<IUrfDemoUnitOfWork, UrfDemoUnitOfWork>();
            services.AddScoped<IRepository<Product>, Repository<Product>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
