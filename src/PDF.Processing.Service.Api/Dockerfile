# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PDF.Processing.Service.slnx", "./"]
COPY ["src/PDF.Processing.Service.Api/PDF.Processing.Service.Api.csproj", "src/PDF.Processing.Service.Api/"]
COPY ["src/PDF.Processing.Service.Infrastructure/PDF.Processing.Service.Infrastructure.csproj", "src/PDF.Processing.Service.Infrastructure/"]
COPY ["src/PDF.Processing.Service.Application/PDF.Processing.Service.Application.csproj", "src/PDF.Processing.Service.Application/"]
COPY ["src/PDF.Processing.Service.Domain/PDF.Processing.Service.Domain.csproj", "src/PDF.Processing.Service.Domain/"]
COPY ["src/PDF.Processing.Service.Contracts/PDF.Processing.Service.Contracts.csproj", "src/PDF.Processing.Service.Contracts/"]

RUN dotnet restore "src/PDF.Processing.Service.Api/PDF.Processing.Service.Api.csproj"

COPY . .
RUN dotnet publish "src/PDF.Processing.Service.Api/PDF.Processing.Service.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PDF.Processing.Service.Api.dll"]

