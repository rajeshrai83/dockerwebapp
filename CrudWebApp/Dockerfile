# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CrudWebApp.csproj", "CrudWebApp/"]
RUN dotnet restore "CrudWebApp/CrudWebApp.csproj"
WORKDIR /src/CrudWebApp
COPY . .
RUN dotnet publish "CrudWebApp.csproj" -c Release -o /app/publish

# Stage 2: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Use the port defined in Program.cs
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CrudWebApp.dll"]