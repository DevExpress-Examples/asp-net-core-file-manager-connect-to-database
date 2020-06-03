using System;
using System.IO;
using FileManagerDB.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileManagerDB {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();
            services.AddMvc();
            services
              .AddMemoryCache()
              .AddSession(s => {
                  s.Cookie.Name = "DevExtreme.NETCore.Demos";
              });
            services.Configure<RouteOptions>(options => options.AppendTrailingSlash = true);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDbContext<FileManagementDbContext>(ConfigureFileManagementContext);
            services.AddTransient<DbFileProvider>();
            services.AddEntityFrameworkSqlite();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
           
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
          
        }


        static void ConfigureFileManagementContext(IServiceProvider serviceProvider, DbContextOptionsBuilder options) {
            var hosting = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var dbPath = Path.Combine(hosting.ContentRootPath, "FileManagement.db");
            options.UseSqlite("Data Source=" + dbPath);
        }
    }
}
