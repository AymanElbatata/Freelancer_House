using AymanFreelance.BLL.Interfaces;
using AymanFreelance.BLL.Repositories;
using AymanFreelance.DAL.BaseEntity;
using AymanFreelance.DAL.Contexts;
using AymanFreelance.DAL.Entities;
using AymanFreelance.PL.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });
builder.Services.AddSession();
builder.Services.AddDbContext<AymanFreelanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddHttpClient<GeoLocationHelper>();


//services.AddSingleton 

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMySPECIALGUID, MySPECIALGUID>();
builder.Services.AddScoped<ICountryTBLRepository, CountryTBLRepository>();
builder.Services.AddScoped<IGenderTBLRepository, GenderTBLRepository>();
builder.Services.AddScoped<IAppErrorTBLRepository, AppErrorTBLRepository>();
builder.Services.AddScoped<IEmailTBLRepository, EmailTBLRepository>();
builder.Services.AddScoped<IProfessionTBLRepository, ProfessionTBLRepository>();
builder.Services.AddScoped<IUserTypeTBLRepository, UserTypeTBLRepository>();
builder.Services.AddScoped<IProjectTBLRepository, ProjectTBLRepository>();
builder.Services.AddScoped<IFreelancerRatingTBLRepository, FreelancerRatingTBLRepository>();
builder.Services.AddScoped<IProjectApplicationTBLRepository, ProjectApplicationTBLRepository>();

//services.AddTransient


builder.Services.AddAutoMapper(m => m.AddProfile(new MappingProfiles()));



builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AymanFreelanceDbContext>()
.AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = new PathString("/Account/Login");
        options.AccessDeniedPath = new PathString("/Home/Error");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
SeedInitialData.SeedData(app);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.UseSession();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error != null)
        {
            var ex = exceptionHandlerPathFeature.Error;

            // Log
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Global exception caught at {Path}", exceptionHandlerPathFeature.Path);

            // Save to DB
            using var scope = context.RequestServices.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAppErrorTBLRepository>();
            repo.Add(new AppErrorTBL
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace ?? "",
                Controller = exceptionHandlerPathFeature.Path ?? "",
                Action = "" // optional
            });
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "An unexpected error occurred. Please try again later."
        }));
    });
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
