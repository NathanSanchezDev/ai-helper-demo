# Use the official .NET 9 runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["AIHelperDemo.csproj", "."]
RUN dotnet restore "AIHelperDemo.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "AIHelperDemo.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "AIHelperDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AIHelperDemo.dll"]