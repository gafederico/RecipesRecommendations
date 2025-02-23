using Microsoft.EntityFrameworkCore;
using RecipesRecommendations.Database;

var builder = WebApplication.CreateBuilder(args);

// Configures the DbContext with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")))
);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Loads the credentials file. 
builder.Configuration.AddJsonFile("credentials.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// Applies migrations automatically when the application starts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
