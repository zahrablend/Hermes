using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HermesChat.Data.Context;
using HermesChat.Data.Models;
using EmailService;
using HermesChat.Web.CustomTokenProviders;
using Serilog;
using Azure.Identity;

namespace HermesChat.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            var keyVaultUrl = new Uri(builder.Configuration.GetSection("KeyVaultURL").Value!);
           
            var azureCredential = new DefaultAzureCredential();

            builder.Configuration.AddAzureKeyVault(keyVaultUrl, azureCredential);


            var connectionString = builder.Configuration.GetSection("HermesChatAzureDb").Value ?? throw new InvalidOperationException("Connection string 'HermesChatContextConnection' not found.");
            // Logger below
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
            // Logger above
            builder.Services.AddDbContext<HermesChatContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<User, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = false;
                opt.Password.RequireUppercase = false;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
            })
                .AddEntityFrameworkStores<HermesChatContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation");

            // Configure a token life span
            builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(2));

            builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromDays(3));

            // Add services to the container
            builder.Services.AddAutoMapper(typeof(Program));

            // register email configuration
            var emailConfig = new EmailConfiguration
            {
                From = config.GetSection("From").Value,
                SmtpServer = config.GetSection("SmtpServer").Value,
                Port = int.Parse(config.GetSection("Port").Value),
                UsernameEmail = config.GetSection("UsernameEmail").Value,
                Password = config.GetSection("Password").Value
            };


            builder.Services.AddSingleton(emailConfig);

            builder.Services.AddScoped<IEmailSender, EmailSender>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration.GetSection("ClientId").Value;
                    options.ClientSecret = builder.Configuration.GetSection("ClientSecret").Value;
                })
             
                .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration.GetSection("AppId").Value;
                    options.AppSecret = builder.Configuration.GetSection("AppSecret").Value;
                });

            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            // Logger below
            app.UseSerilogRequestLogging();
            // Logger above
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chat");
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Chat}/{id?}");

            app.Run();
        }
    }
}