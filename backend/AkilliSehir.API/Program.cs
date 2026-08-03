using AkilliSehir.API.Security;
using AkilliSehir.Business.Abstract;
using AkilliSehir.Business.Concrete;
using AkilliSehir.DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

const string FlutterWebCorsPolitikasi = "FlutterWeb";

// JWT ayarları uygulama başlarken doğrulanır. Production ortamında Anahtar
// appsettings'e yazılmamalı; Auth__Jwt__Anahtar ortam değişkeninden gelmelidir.
var jwtIssuer = builder.Configuration["Auth:Jwt:Issuer"]
    ?? throw new InvalidOperationException("Auth:Jwt:Issuer yapılandırması tanımlanmalıdır.");
var jwtAudience = builder.Configuration["Auth:Jwt:Audience"]
    ?? throw new InvalidOperationException("Auth:Jwt:Audience yapılandırması tanımlanmalıdır.");
var jwtAnahtari = builder.Configuration["Auth:Jwt:Anahtar"]
    ?? throw new InvalidOperationException(
        "Auth:Jwt:Anahtar ortam değişkeni veya gizli depoda tanımlanmalıdır.");

if (Encoding.UTF8.GetByteCount(jwtAnahtari) < 32)
{
    throw new InvalidOperationException("Auth:Jwt:Anahtar en az 32 bayt uzunluğunda olmalıdır.");
}

// Flutter Web geliştirme sunucusunun localhost üzerindeki farklı portlardan
// API'ye istek gönderebilmesine izin verilir.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FlutterWebCorsPolitikasi, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttp
                && uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Bağlantı bilgisi kod içine yazılmaz; appsettings.json üzerinden okunur.
var connectionString = builder.Configuration.GetConnectionString("CityDb")
    ?? throw new InvalidOperationException(
        "CityDb bağlantı cümlesi appsettings.json dosyasında bulunamadı.");

// CityDbContext, istek başına (scoped) SQL Server bağlantısıyla oluşturulur.
builder.Services.AddDbContext<CityDbContext>(options =>
    options.UseSqlServer(connectionString));

// Servis sözleşmeleri ve somut yöneticileri istek başına oluşturulur.
builder.Services.AddScoped<IArizaService, ArizaManager>();
builder.Services.AddScoped<IDuyuruService, DuyuruManager>();
builder.Services.AddScoped<IRandevuService, RandevuManager>();
builder.Services.AddScoped<IKullaniciService, KullaniciManager>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IArizaYonlendirmeService, ArizaYonlendirmeManager>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// API uç noktalarında rol denetimi, istemci ekranına değil imzalı JWT'ye dayanır.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtari)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// Mobil istemcinin enumları anlaşılır metin olarak göndermesi ve alması sağlanır.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Geliştirme ortamında tanımlı demo saha hesabı, ilk çalıştırmada bir kez oluşturulur.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.EnsureDevelopmentStaffAsync();
}

// Flutter Web istemcisi yerelde http://localhost:5000 adresine bağlanır.
// Bu nedenle geliştirme ortamında HTTP isteği HTTPS'e yönlendirilmez.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(FlutterWebCorsPolitikasi);
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Swagger tüm ortamlarda (Development ve Production) aktif kalır.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// API'nin ayakta olduğunu doğrulamak için basit uç nokta.
app.MapGet("/", () => Results.Ok(new
{
    Mesaj = "Akıllı Şehir API çalışıyor."
}));

app.Run();
