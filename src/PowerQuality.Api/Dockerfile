FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PowerQuality.Domain/PowerQuality.Domain.csproj", "src/PowerQuality.Domain/"]
COPY ["src/PowerQuality.Application/PowerQuality.Application.csproj", "src/PowerQuality.Application/"]
COPY ["src/PowerQuality.Infrastructure/PowerQuality.Infrastructure.csproj", "src/PowerQuality.Infrastructure/"]
COPY ["src/PowerQuality.Worker/PowerQuality.Worker.csproj", "src/PowerQuality.Worker/"]
COPY ["src/PowerQuality.Api/PowerQuality.Api.csproj", "src/PowerQuality.Api/"]
RUN dotnet restore "src/PowerQuality.Api/PowerQuality.Api.csproj"
COPY . .
RUN dotnet publish "src/PowerQuality.Api/PowerQuality.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PowerQuality.Api.dll"]
