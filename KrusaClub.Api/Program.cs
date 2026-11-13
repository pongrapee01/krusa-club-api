using System.Text;
using KrusaClub.Api.Auth;
using KrusaClub.Api.Configuration;
using KrusaClub.Api.Features.Master;
using KrusaClub.Api.Features.Master.Caching;
using KrusaClub.Api.Features.Master.Repositories;
using KrusaClub.Api.Features.Menus;
using KrusaClub.Api.Features.Users;
using KrusaClub.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection(SupabaseOptions.SectionName));
builder.Services.Configure<MasterCacheOptions>(builder.Configuration.GetSection(MasterCacheOptions.SectionName));
builder.Services.AddMemoryCache();

var supabaseOptions = builder.Configuration.GetSection(SupabaseOptions.SectionName).Get<SupabaseOptions>()
    ?? throw new InvalidOperationException("Supabase configuration is missing.");

// --- ตั้งค่า CORS (เพื่อให้ React ยิง API เข้ามาได้) ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // ใส่ URL ของ React คุณ
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var signingMode = string.IsNullOrWhiteSpace(supabaseOptions.JwtSigningMode)
    ? "Jwks"
    : supabaseOptions.JwtSigningMode.Trim();

if (string.Equals(signingMode, "Hmac", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(supabaseOptions.JwtSecret))
    {
        throw new InvalidOperationException("Supabase:JwtSecret is required when JwtSigningMode is Hmac.");
    }

    var jwtIssuers = BuildSupabaseJwtIssuers(supabaseOptions);
    if (jwtIssuers.Count == 0)
    {
        throw new InvalidOperationException(
            "No JWT issuers resolved. Set Supabase:JwtIssuer to https://<project-ref>.supabase.co/auth/v1 " +
            "or use pooler username postgres.<project-ref> in ConnectionString.");
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => ConfigureSupabaseJwtBearerHmac(options, supabaseOptions, jwtIssuers));
}
else
{
    var authority = ResolveSupabaseJwtAuthority(supabaseOptions)
        ?? throw new InvalidOperationException(
            "Cannot resolve JWT Authority. Set Supabase:JwtIssuer to https://<project-ref>.supabase.co/auth/v1 " +
            "or set ProjectRef / use pooler username postgres.<project-ref> in ConnectionString.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => ConfigureSupabaseJwtBearerJwks(options, supabaseOptions, authority));
}

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddSingleton<MasterDataCache>();
builder.Services.AddHostedService<MasterCacheWarmupHostedService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<IMasterRoleRepository, MasterRoleRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IPeriodRepository, PeriodRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IAttendanceStatusRepository, AttendanceStatusRepository>();
builder.Services.AddScoped<ISystemStatusRepository, SystemStatusRepository>();
builder.Services.AddScoped<IMasterMenuRepository, MasterMenuRepository>();
builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddSingleton(_ => CreateSupabaseDataSource(supabaseOptions.ConnectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static NpgsqlDataSource CreateSupabaseDataSource(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    // Poolers (e.g. Supabase Supavisor) may reject GSS; Npgsql 10.0.2 can leave pooled connectors broken
    // (ObjectDisposedException on ManualResetEventSlim) after a failed GSS negotiation.
    if (!connectionString.Contains("GssEncryption", StringComparison.OrdinalIgnoreCase))
    {
        builder.GssEncryptionMode = GssEncryptionMode.Disable;
    }

    // Supabase pooler already handles pooling server-side.
    // Disabling client pooling avoids stale pooled sockets ("Exception while reading from stream")
    // that can happen after page reloads / bursty reconnects.
    if (!string.IsNullOrWhiteSpace(builder.Host) &&
        builder.Host.Contains(".pooler.supabase.com", StringComparison.OrdinalIgnoreCase))
    {
        builder.Pooling = false;
        builder["Keepalive"] = 30;
    }

    return new NpgsqlDataSourceBuilder(builder.ConnectionString).Build();
}

static void ConfigureSupabaseJwtBearerHmac(
    JwtBearerOptions options,
    SupabaseOptions supabaseOptions,
    IReadOnlyList<string> jwtIssuers)
{
    options.RequireHttpsMetadata = false;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseOptions.JwtSecret)),
        ValidateIssuer = true,
        ValidIssuers = jwtIssuers,
        ValidateAudience = true,
        ValidAudiences = BuildSupabaseJwtAudiences(supabaseOptions),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "sub",
        RoleClaimType = "role"
    };

    AttachSupabaseJwtBearerDiagnostics(options);
}

static void ConfigureSupabaseJwtBearerJwks(
    JwtBearerOptions options,
    SupabaseOptions supabaseOptions,
    string authority)
{
    options.Authority = authority;
    options.RequireHttpsMetadata = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudiences = BuildSupabaseJwtAudiences(supabaseOptions),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "sub",
        RoleClaimType = "role"
    };

    AttachSupabaseJwtBearerDiagnostics(options);
}

static void AttachSupabaseJwtBearerDiagnostics(JwtBearerOptions options)
{
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer.Supabase");
            logger.LogWarning(context.Exception, "JWT authentication failed");

            context.HttpContext.Response.Headers.Append("x-auth-error", context.Exception.GetType().Name);
            if (!string.IsNullOrWhiteSpace(context.Exception.Message))
            {
                context.HttpContext.Response.Headers.Append(
                    "x-auth-error-message",
                    context.Exception.Message.Replace('\r', ' ').Replace('\n', ' '));
            }

            return Task.CompletedTask;
        }
    };
}

static string? ResolveSupabaseJwtAuthority(SupabaseOptions options)
{
    if (!string.IsNullOrWhiteSpace(options.JwtIssuer) &&
        options.JwtIssuer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return options.JwtIssuer.Trim().TrimEnd('/');
    }

    var projectRef = !string.IsNullOrWhiteSpace(options.ProjectRef)
        ? options.ProjectRef.Trim()
        : TryGetSupabaseProjectRefFromConnectionString(options.ConnectionString);

    if (!string.IsNullOrWhiteSpace(projectRef))
    {
        return $"https://{projectRef}.supabase.co/auth/v1";
    }

    return null;
}

static List<string> BuildSupabaseJwtIssuers(SupabaseOptions options)
{
    var set = new HashSet<string>(StringComparer.Ordinal);

    if (!string.IsNullOrWhiteSpace(options.JwtIssuer))
    {
        var configuredIssuer = options.JwtIssuer.Trim();
        if (!string.Equals(configuredIssuer, "supabase", StringComparison.OrdinalIgnoreCase))
        {
            set.Add(configuredIssuer);
        }
    }

    var projectRef = !string.IsNullOrWhiteSpace(options.ProjectRef)
        ? options.ProjectRef.Trim()
        : TryGetSupabaseProjectRefFromConnectionString(options.ConnectionString);

    if (!string.IsNullOrWhiteSpace(projectRef))
    {
        set.Add($"https://{projectRef}.supabase.co/auth/v1");
    }

    return set.ToList();
}

static List<string> BuildSupabaseJwtAudiences(SupabaseOptions options)
{
    var primary = string.IsNullOrWhiteSpace(options.JwtAudience) ? "authenticated" : options.JwtAudience.Trim();
    var set = new HashSet<string>(StringComparer.Ordinal) { primary, "authenticated" };
    return set.ToList();
}

static string? TryGetSupabaseProjectRefFromConnectionString(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var user = builder.Username;
    if (string.IsNullOrWhiteSpace(user))
    {
        return null;
    }

    const string prefix = "postgres.";
    if (user.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && user.Length > prefix.Length)
    {
        return user[prefix.Length..];
    }

    return null;
}
