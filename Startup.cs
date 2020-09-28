using ControleDeLeiloes.Areas.Identity;
using ControleDeLeiloes.Configuration;
using ControleDeLeiloes.Data;
using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ControleDeLeiloes
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            var builer = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            //if (env.IsProduction())
            //{
            builer.AddUserSecrets<Startup>();
            //}
            Configuration = builer.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextConfig(Configuration); // In DbContextConfig
            services.AddIdentityConfig(Configuration); // In IdentityConfig
            services.AddMvcAndRazor(); // In MvcAndRazor
            services.AddDependencyInjectConfig(Configuration); // In DependencyInjectConfig
        }

        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            ControleDeLeiloesDbContext context,
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            UsuarioRoleDefault.Seed(context, userManager, roleManager).Wait();

            app.UseEndpoints(endpoints =>
            {
                //routes.MapRoute("modulos","Prontuario/{controller=Home}/{action=Index}/{id?}");

                //routes.MapRoute("pacientes","{controller=Home}/{action=Index}/{id}/{paciente}");

                //endpoints.MapAreaControllerRoute(
                //    name: "logout",
                //    areaName: "Identity",
                //    pattern: "Identity/Account/Logout");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                endpoints.MapRazorPages();

            });

            //app.UseMvc(routes =>
            //{
            //    //routes.MapRoute("modulos","Prontuario/{controller=Home}/{action=Index}/{id?}");
            //    //routes.MapRoute("pacientes","{controller=Home}/{action=Index}/{id}/{paciente}");

            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
