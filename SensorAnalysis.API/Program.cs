using SensorAnalysis.API.Extensions;
using SensorAnalysis.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Sensor Analysis API",
        Version     = "v1",
        Description = "Async processing of environmental sensor samples with anomaly detection."
    });
});


builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddSensorAnalysis(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensor Analysis API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
