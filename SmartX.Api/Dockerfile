# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SmartX.Api/SmartX.Api.csproj SmartX.Api/
RUN dotnet restore SmartX.Api/SmartX.Api.csproj
COPY SmartX.Api/ SmartX.Api/
WORKDIR /src/SmartX.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartX.Api.dll"]
