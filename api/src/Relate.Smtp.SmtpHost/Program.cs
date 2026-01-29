using Relate.Smtp.Infrastructure;
using Relate.Smtp.Infrastructure.Services;
using Relate.Smtp.SmtpHost;
using Relate.Smtp.SmtpHost.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add infrastructure services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=relate-smtp.db";

builder.Services.AddInfrastructure(connectionString);

// Configure HTTP client for notification service
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient<IEmailNotificationService, HttpEmailNotificationService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Configure SMTP server options
builder.Services.Configure<SmtpServerOptions>(
    builder.Configuration.GetSection("Smtp"));

// Add SMTP server hosted service
builder.Services.AddHostedService<SmtpServerHostedService>();

var host = builder.Build();
host.Run();
