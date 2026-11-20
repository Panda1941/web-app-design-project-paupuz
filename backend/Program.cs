using System.Reflection;
using LolBuilds.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Database (SQLite file) ----------
string connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=lolbuilds.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

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
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// ---------- CORS (the React frontend runs on a different origin) ----------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
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
app.MapControllers();

app.Run();
