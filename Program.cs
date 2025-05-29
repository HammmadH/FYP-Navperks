using FYP_Navperks.Models.Admin;
using Microsoft.Extensions.Configuration;
using FYP_Navperks.Services;
using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Parking;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy to allow any origin, method, and header
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Pass configuration to DbHelper via dependency injection
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHttpClient<SlotHardwareService>();
builder.Services.AddScoped<ISlotHardwareService, SlotHardwareService>();



var app = builder.Build();

app.MapHub<ParkingHub>("/parkingHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
