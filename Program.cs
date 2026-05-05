using Microsoft.EntityFrameworkCore;
using WikiKnowledge.CallServices;
using WikiKnowledge.Data;
using WikiKnowledge.WikipediaServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<WikipediaSummary>();
builder.Services.AddHttpClient<LanguageLink>();
builder.Services.AddScoped<Services>();

//builder.Services.AddAuthentication()
//    .AddGoogleOpenIdConnect(googleOptions =>
//    {
//        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//    });

builder.Services.AddDbContext<CronologiaContext>(options =>
{
    options.UseSqlServer("connectionString");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
