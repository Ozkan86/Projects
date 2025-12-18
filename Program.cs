using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Eventify.Data;
using Eventify.Services;
using Eventify.Middleware;
using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;





Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console() 
    .WriteTo.File("Logs/eventify_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEventService, EventService>(); //DI konteynırına servis eklendi
builder.Services.AddScoped<IUserService, UserService>(); //DI konteynırına servis eklendi
builder.Services.AddScoped<IAttendanceService, AttendanceService>(); //DI konteynırına servis eklendi
builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers()
    ;
builder.Services.AddApiVersioning(options =>
{
    // API sürümlerini 'api-supported-versions' ve 'api-deprecated-versions' başlıklarında raporla
    options.ReportApiVersions = true;

    // Bir istemci belirtmediğinde varsayılan sürüm (ör. 1.0)
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // İstemci belirtmediğinde varsayılan sürümü varsay
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Sürümün istekten nasıl okunacağını tanımla (URL segmenti kullanacağız)
    options.ApiVersionReader = new UrlSegmentApiVersionReader();

}).AddApiExplorer(options => // Swagger/OpenAPI entegrasyonu için bunu ekle
{
    // Sürümü "'v'major[.minor][-status]" olarak biçimlendir (ör. v1.0, v2.0-beta)
    options.GroupNameFormat = "'v'VVV";

    // Rota şablonunda sürümü değiştir
    options.SubstituteApiVersionInUrl = true;
});
// --- API Sürümleme Yapılandırmasının Sonu ---



// Swagger desteği eklendi
builder.Services.AddEndpointsApiExplorer(); // Swagger endpoint tanımlayıcısı
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    /*c.SwaggerDoc("v1", new OpenApiInfo { Title = "Eventify API", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Eventify API", Version = "v2" });*/
    // 🔐 JWT desteği
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token. Sadece tokenı yazın, örn: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });


    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Swagger UI oluşturucu
//-------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:5173") // Vite'ın default portu
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

//--------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //Kimlik doğrulama servislerini kaydeder ve varsayılan kimlik doğrulama şemasını JWT Bearer olarak ayarlar.
    .AddJwtBearer(options => //JWT Bearer kimlik doğrulama şemasını yapılandırır
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], //JWT'nin geçerli yayıncısını doğrular
            ValidAudience = builder.Configuration["Jwt:Audience"], //JWT'nin geçerli alıcısını doğrular
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])), //JWT'nin imzasını doğrulamak için kullanılan anahtar
            RoleClaimType = ClaimTypes.Role // JWT'deki rol iddialarını doğrulamak için kullanılan iddia türü
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>(); //kendi yazdığımız middleware bileşenini dahil ediyoruz
app.UseSecurityHeaders(); //Güvenlik başlıklarını ayarlamak için middleware'i kullanıyoruz
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    // Swagger middleware'leri sadece geliştirme ortamında çalıştırılır
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); // Genel bir hata sayfasına/işleyicisine yönlendirir
    app.UseHsts(); // Üretimde HSTS'yi zorunlu kıl
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication(); //Kimlik doğrulama middleware'ini ekledik (Yetkilendirmeden önce olmalı)
app.UseAuthorization(); //Yetkilendirme middleware'ini ekledik
app.MapControllers();

app.Run();
