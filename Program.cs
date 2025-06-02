using FYP_Navperks.Models.Admin;
using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Parking;
using FYP_Navperks.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<DbHelper>();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHttpClient<SlotHardwareService>();
builder.Services.AddScoped<ISlotHardwareService, SlotHardwareService>();

var app = builder.Build();

app.MapHub<ParkingHub>("/parkingHub");

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
