using api.Data;
using api.Interfaces;
using api.Models;
using api.Repository;
using api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddOpenApi();  //app.MapOpenApi() yerine app.UseSwagger() ve app.UseSwaggerUI(): Swagger/OpenAPI servisini düzgün başlatır.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------------------------------------------------------AUTHORIZE--------------------------------------------
//swagger have jwt built in with this code (just copy and paste it)  
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http, //ApiKey
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    //SONRADAN!!!!!!!!!!!!!!!
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// to prevent object cycles. 
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});


builder.Services.AddDbContext<ApplicationDBContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    //you can add more restrictions
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
})
.AddEntityFrameworkStores<ApplicationDBContext>();


// -----------------------------------------------------------------------------------------AUTHENTİCATION-------------------------------------------------
//we are gonna add schemes (jwt or cookies)  (first add nuget Microsoft.AspNetCore.Authentication.JwtBearer)
builder.Services.AddAuthentication(options => {
    //you could make everything default
    options.DefaultAuthenticateScheme = 
    options.DefaultChallengeScheme = 
    options.DefaultForbidScheme =
    options.DefaultScheme = 
    options.DefaultSignInScheme = 
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"], //we have make this within appsettings.json
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        ),
        //SONRADAN!!!!!!!!!!!!!!
        ValidateLifetime = true // Token'ın süresini kontrol eder 
    };

        // SONRADAN!!!!!!!!!!!!!!!!!!!
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token successfully validated.");
                return Task.CompletedTask;
            }
        };    
});

// DEPENDENCY INJECTIONS
builder.Services.AddScoped<IStockRepository ,StockRepository>();
builder.Services.AddScoped<ICommentRepository ,CommentRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IFMPService, FMPService>();
builder.Services.AddHttpClient<IFMPService, FMPService>();

var app = builder.Build();
// AFTER THE BUİLDER BUİLD DOWN BELOW İS MİDDLEWARE 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi(); //program açılışında bu vardı ama swagger kullanmak için değiştirdik
    app.UseSwagger();
    //options ile end point eklemediğimde swagger sayfası otomatik açılmıyor ayrıca launcSettings.json doyasında "launchBrowser": true,  olmalı 
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.RoutePrefix = string.Empty;  //// Bu ayar Swagger arayüzünü varsayılan yapar.
    });
}

app.UseHttpsRedirection();

//it has to be under the UseHttpsRedirection 
//CORS (Cross-Origin Resource Sharing) -> web uyg başka alan adı, protokol veya port üzerinde bulunan kaynaklara(API, resim, veri vb.)erişmesine izin ver veya kısıtlamak için kul güvenlik mekanizması
// birçok modern uygulama, farklı bir alan adında veya portta barındırılan API'lerle iletişim kurar. İşte burada CORS devreye girer ve bu tür taleplerin nasıl işleneceğini kontrol eder.
//CORS bir güvenlik duvarı değildir. Sunucuyu kötü amaçlı isteklerden korumaz.
app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials() //kimlik bilgileri (çerezler) ile erişim sağlanır.
        //.WithOrigins("https://localhost:44351)  //here is when you deploy this is where u set the actual domain when u deploying
        .SetIsOriginAllowed(origin => true) // Tüm origin'lere izin verir
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); //if we dont do this swagger wont work (you get https redirect error)

app.Run();
