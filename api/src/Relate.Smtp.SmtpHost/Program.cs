using Relate.Smtp.Infrastructure;
using Relate.Smtp.SmtpHost;

var builder = Host.CreateApplicationBuilder(args);

// Add infrastructure services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=relate-smtp.db";

builder.Services.AddInfrastructure(connectionString);

// Configure SMTP server options
builder.Services.Configure<SmtpServerOptions>(
    builder.Configuration.GetSection("Smtp"));

// Add SMTP server hosted service
builder.Services.AddHostedService<SmtpServerHostedService>();

var host = builder.Build();
host.Run();
