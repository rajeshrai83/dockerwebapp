# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CrudApi.csproj", "CrudApi/"]
RUN dotnet restore "CrudApi/CrudApi.csproj"
WORKDIR /src/CrudApi
COPY . .
RUN dotnet publish "CrudApi.csproj" -c Release -o /app/publish

# Stage 2: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Use the port defined in Program.cs
ENV ASPNETCORE_URLS=http://+:8080 
ENTRYPOINT ["dotnet", "CrudApi.dll"]