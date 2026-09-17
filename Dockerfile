# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo .csproj DESDE la carpeta EcommerceApp
COPY ["EcommerceApp/EcommerceApp.csproj", "EcommerceApp/"]
RUN dotnet restore "EcommerceApp/EcommerceApp.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/EcommerceApp"
RUN dotnet build "EcommerceApp.csproj" -c Release -o /app/build

# Etapa 2: Publicación
FROM build AS publish
RUN dotnet publish "EcommerceApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EcommerceApp.dll"]