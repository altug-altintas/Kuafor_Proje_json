using Proje_web.Models.AutoMappers;
using Proje_Dal.Context;
using Proje_Dal.Repositories.Abstract;
using Proje_Dal.Repositories.Concrete;
using Proje_Dal.Repositories.Interfaces.Abstract;
using Proje_Dal.Repositories.Interfaces.Concrete;
using Proje_model.Models.Concrete;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Proje_Dal.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Proje_web
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
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
            });




            services.AddControllersWithViews();

            services.AddDbContext<ProjectContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("Default")));
            services.AddIdentity<AppUser, IdentityRole>
                (
                    x =>
                    {
                        x.User.RequireUniqueEmail = true;    //ki�iye ait mail adresi e�siz olcak 
                        x.Password.RequiredLength = 4;    // �ifre karakter say�s�
                        x.Password.RequireLowercase = false;   // k���k harf zorunlulu�u olsun olmas�n
                        x.Password.RequireUppercase = false;   // b�y�k harf zorlunlu�u olsun olmasn
                        x.Password.RequireNonAlphanumeric = false;   // �ifrede �rne�in !, @, #, $, vb. gibi �zel karakterler olsun olmas�n
                        x.Password.RequireDigit = false;    // en az bir say� olsun olmasn
                        x.Password.RequiredUniqueChars = 0;

                        // katmanl� mimari  migration yaparken  package manager alan�nda  default proje   project context oldu�u yer start set projede  ba�lant� bilgilerinin oldu�u bu proje olmal�


                    }

                ).AddEntityFrameworkStores<ProjectContext>().AddDefaultTokenProviders();

            services.AddControllers()
           .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.PropertyNamingPolicy = null; // C# ve JSON property isimlerinin e�le�mesi i�in
           });


            services.AddAutoMapper(typeof(Mappers));
            services.AddScoped<IAppUserRepo, AppUserRepo>();
            services.AddScoped<IIslemleRepo, IslemleRepo>();
            services.AddScoped<IKuaforTakvimRepo, KuaforTakvimRepo>();
            services.AddScoped<IKuaforPersoneliRepo, KuaforPersoneliRepo>();
            services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
            services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

            var appSettingSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingSection);

            var appSettings = appSettingSection.Get<AppSettings>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Varsay�lan kimlik do�rulama �emas�
                options.DefaultSignInScheme = "CookieAuth"; // SignIn i�in �zel �ema
            })
                  .AddCookie("CookieAuth", options =>
              {
                  options.Cookie.Name = "MyAuthCookie"; // Cookie ad�
                  options.LoginPath = "/Account/Login"; // Giri� sayfas� yolu
                  options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eri�im sayfas� yolu
                  options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie s�resi
              });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAnyOrigin"); // CORS'u uyguluyoruz

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //localhost/----areaname----/controllername/actionname/paramtere �eklilnde arealar olmal� 

                endpoints.MapControllerRoute(name: "area", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
