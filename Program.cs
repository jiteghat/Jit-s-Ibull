using iBull.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using iBull.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.WebSockets;
using iBull.Controllers;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<APIDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnectionString"),
    sqlServerOptionsAction: sqlOptions =>
    sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)
    ));
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Constants.JWT_SECURITY_KEY)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
        });
});

builder.Services.AddTransient<FileServices>();
builder.Services.AddScoped<iBull.Data.OptionCalculation>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
//app.UseWebSockets();
//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/ws")
//    {
//        if (context.WebSockets.IsWebSocketRequest)
//        {
//            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
//            HandleWebSocket(webSocket);
//        }
//        else
//        {
//            context.Response.StatusCode = 400; // Bad Request
//        }
//    }
//    else
//    {
//        await next();
//    }
//});

//async Task HandleWebSocket(WebSocket webSocket)
//{
//    byte[] buffer = new byte[1024];

//    while (webSocket.State == WebSocketState.Open)
//    {
//        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

//        if (result.MessageType == WebSocketMessageType.Text)
//        {
//            // Handle text messages received from the WebSocket client
//            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
//            Console.WriteLine($"Received message: {message}");

//            // Handle your custom logic for incoming messages

//            // Send a response back
//            string responseMessage = "Received: " + message;
//            byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
//            await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
//        }
//        else if (result.MessageType == WebSocketMessageType.Close)
//        {
//            // Handle WebSocket close message
//            break;
//        }
//    }

//    // Clean up resources when the WebSocket connection is closed
//    //await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
//    await webSocket.CloseAsync(WebSocketCloseStatus.Empty, "Test Cancelled", CancellationToken.None);
//}
