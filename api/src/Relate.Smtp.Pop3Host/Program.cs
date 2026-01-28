using Relate.Smtp.Infrastructure;
using Relate.Smtp.Pop3Host;
using Relate.Smtp.Pop3Host.Handlers;

var builder = Host.CreateApplicationBuilder(args);

// Add infrastructure services (database + repositories)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=relate-smtp.db";
builder.Services.AddInfrastructure(connectionString);

// Configure POP3 server options
builder.Services.Configure<Pop3ServerOptions>(
    builder.Configuration.GetSection("Pop3"));

// Register handlers (scoped per connection)
builder.Services.AddScoped<Pop3UserAuthenticator>();
builder.Services.AddScoped<Pop3CommandHandler>();
builder.Services.AddScoped<Pop3MessageManager>();

// Register hosted service
builder.Services.AddHostedService<Pop3ServerHostedService>();

var host = builder.Build();
host.Run();
