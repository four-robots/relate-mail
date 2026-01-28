# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/Relate.Smtp.Core/Relate.Smtp.Core.csproj src/Relate.Smtp.Core/
COPY src/Relate.Smtp.Infrastructure/Relate.Smtp.Infrastructure.csproj src/Relate.Smtp.Infrastructure/
COPY src/Relate.Smtp.Api/Relate.Smtp.Api.csproj src/Relate.Smtp.Api/
COPY src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj src/Relate.Smtp.SmtpHost/
COPY src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj src/Relate.Smtp.Pop3Host/
RUN dotnet restore src/Relate.Smtp.Api/Relate.Smtp.Api.csproj
RUN dotnet restore src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj
RUN dotnet restore src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/Relate.Smtp.Api/Relate.Smtp.Api.csproj -c Release -o /app/api
RUN dotnet publish src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj -c Release -o /app/smtp
RUN dotnet publish src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj -c Release -o /app/pop3

# API runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /app/api .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Relate.Smtp.Api.dll"]

# SMTP runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS smtp
WORKDIR /app
COPY --from=build /app/smtp .
EXPOSE 587 465
ENTRYPOINT ["dotnet", "Relate.Smtp.SmtpHost.dll"]

# POP3 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS pop3
WORKDIR /app
COPY --from=build /app/pop3 .
EXPOSE 110 995
ENTRYPOINT ["dotnet", "Relate.Smtp.Pop3Host.dll"]
