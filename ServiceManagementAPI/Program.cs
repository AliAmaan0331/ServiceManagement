using Microsoft.AspNetCore.Authorization;
using ServiceManagementAPI.Authorization;
using ServiceManagementAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessToken", policy =>
    {
        policy.AddRequirements(new APIRequirement());
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthorizationHandler, APIHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(options =>
                options.WithOrigins("https://localhost:44465")
                .AllowAnyHeader()
                .AllowAnyMethod());
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();
