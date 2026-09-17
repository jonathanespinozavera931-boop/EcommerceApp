FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EcommerceApp.csproj", "./"]
RUN dotnet restore "EcommerceApp.csproj"
COPY . .
RUN dotnet build "EcommerceApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcommerceApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet EcommerceApp.dll"]