# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy CSPROJ and restore as distinct layers
COPY ["FYP-Navperks/FYP-Navperks.csproj", "FYP-Navperks/"]
RUN dotnet restore "FYP-Navperks/FYP-Navperks.csproj"

# Copy everything else
COPY . .

WORKDIR "/src/FYP-Navperks"
RUN dotnet publish "FYP-Navperks.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FYP-Navperks.dll"]
