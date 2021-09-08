using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using ChatApp.Web.RabbitMQ;
using ChatApp.Bussiness.Helpers;
using ChatApp.Bussiness.AutoMapper;
using ChatApp.Bussiness.Concrete;
using ChatApp.Data.Context;
using ChatApp.Data.Entities.Concrete;
using ChatApp.Bussiness.Abstract;

namespace ChatApp.Web
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
            services.AddDbContext<ChatAppWebContext>(options =>
                   options.UseNpgsql(Configuration.GetConnectionString("ChatAppWebContextConnection"), b => b.MigrationsAssembly("ChatApp.Web")), ServiceLifetime.Transient);

            services.AddDefaultIdentity<ApplicationUser>(options => { options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<ChatAppWebContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSignalR();
            services.AddScoped<AddMessageToQeueue>();
            services.AddTransient<IChatManager,ChatManager>();
            services.AddAutoMapper(typeof(MessageProfile));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<ChatHub>("/ChatHub");
            });
        }
    }
}
