using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using access.analyser.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace access.analyser
{
    public class Startup
    {
        public Startup (IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext> (options =>
                 options.UseNpgsql (
                     Configuration.GetConnectionString ("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser> ()
                .AddRoles<IdentityRole> ()
                .AddEntityFrameworkStores<ApplicationDbContext> ();
            services.AddControllersWithViews ();
            services.AddRazorPages ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext context)
        {
            if (env.IsDevelopment ())
            {
                app.UseDeveloperExceptionPage ();
                app.UseDatabaseErrorPage ();
            }
            else
            {
                app.UseExceptionHandler ("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts ();
            }
            app.UseHttpsRedirection ();
            app.UseStaticFiles ();

            app.UseRouting ();

            app.UseAuthentication ();
            app.UseAuthorization ();

            app.UseEndpoints (endpoints =>
             {
                 endpoints.MapControllerRoute (
                     name: "default",
                     pattern: "{controller=Home}/{action=Index}/{id?}");
                 endpoints.MapRazorPages ();
             });

            context.Database.Migrate ();

            //Ensure Admin role is created on startup
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory> ().CreateScope ();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>> ();
            if (!roleManager.RoleExistsAsync ("Admin").Result)
            {
                var roleResult = roleManager.CreateAsync (new IdentityRole ("Admin")).Result;
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException ("Unable to create role.");
                }
            }

            //Add default user if there is no admin
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>> ();
            var admins = userManager.GetUsersInRoleAsync ("Admin").Result;
            if (admins.Count == 0)
            {
                var adminUser = new IdentityUser () { UserName = "admin@example.com", Email = "admin@example.com" };
                var res = userManager.CreateAsync (adminUser, "1qazXSW@").Result;
                if (!res.Succeeded)
                {
                    throw new InvalidOperationException ("Cannot create admin user");
                }
                adminUser = userManager.FindByNameAsync ("admin@example.com").Result;
                res = userManager.AddToRoleAsync (adminUser, "Admin").Result;
                if (!res.Succeeded)
                {
                    throw new InvalidOperationException ("Cannot create admin user");
                }
            }
        }
    }
}
