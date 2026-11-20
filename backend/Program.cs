using System.Reflection;
using System.Text;
using LolBuilds.Api.Data;
using LolBuilds.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- Database (SQLite file) ----------
string connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=lolbuilds.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// ---------- Authentication (JWT) ----------
// Every request with "Authorization: Bearer <token>" gets its token checked here:
// signature (was it made with our secret key?), issuer, audience and expiry. If all is fine,
// the claims inside the token become HttpContext.User (used by [Authorize] and CurrentUserId).
string jwtKey = TokenService.GetSigningKey(builder.Configuration); // fails at startup if the key is missing
builder.Services.AddSingleton<TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep claim names as they are in the token ("sub", "role")
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            NameClaimType = "unique_name",
            RoleClaimType = "role", // makes [Authorize(Roles = "Admin")] read the "role" claim
        };
    });
builder.Services.AddAuthorization();

// ---------- Controllers + validation responses ----------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Invalid request body:
        //  - 400 Bad Request when the JSON can't be read at all (syntax error, wrong type, empty body)
        //  - 422 Unprocessable Entity when the JSON is fine but values break validation rules
        options.InvalidModelStateResponseFactory = context =>
        {
            bool unreadableJson = false;
            foreach (string key in context.ModelState.Keys)
            {
                if (key == "" || key.StartsWith("$"))
                    unreadableJson = true;
            }

            var problem = new ValidationProblemDetails(context.ModelState);
            if (unreadableJson)
            {
                problem.Status = StatusCodes.Status400BadRequest;
                problem.Title = "The request body is not valid JSON or has wrong value types.";
                return new BadRequestObjectResult(problem);
            }

            problem.Status = StatusCodes.Status422UnprocessableEntity;
            return new UnprocessableEntityObjectResult(problem);
        };
    });

builder.Services.AddProblemDetails();

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LoL Builds API",
        Version = "v1",
        Description =
            "REST API for creating and discussing League of Legends champion builds. " +
            "Resources form the hierarchy Champion -> Build -> Comment. " +
            "Lists support paging (page, pageSize) and filters; every resource has hypermedia links. " +
            "Log in with POST /api/v1/auth/login (e.g. faker_fan / Password123!, or admin / Password123!) " +
            "and send the access token as 'Authorization: Bearer <token>'.",
    });

    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    // Adds the "Authorize" button to Swagger UI so you can paste an access token.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken from POST /api/v1/auth/login.",
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
    });
});

// ---------- CORS ----------
// The React frontend runs on a different origin (domain/port), so the browser only lets it read
// API responses if the API allows that origin. Only the origins listed in Cors:AllowedOrigins
// (comma separated) are allowed, e.g. http://localhost:5173 locally and the real frontend URL in production.
string[] allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Create the database and fill it with demo data on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

// Any unexpected exception becomes a JSON ProblemDetails response instead of an HTML error page.
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication(); // reads and checks the token
app.UseAuthorization();  // applies [Authorize] rules
app.MapControllers();

app.Run();
