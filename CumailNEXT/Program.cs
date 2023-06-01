using CumailNEXT.Implementation.Core;
using CumailNEXT.Implementation.Hubs;

var builder = WebApplication.CreateBuilder(args);

const string CORS_POLICY = "corspolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_POLICY,
        b =>
        {
            b
                // .WithOrigins("*")
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
// builder.Services.AddSignalRCore();

var app = builder.Build();
app.UseCors(CORS_POLICY);


// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();
app.MapHub<DefaultChatHub>("/chatws");
//---------------------------------
Engine.Init();

app.Run();