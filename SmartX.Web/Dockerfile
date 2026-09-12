# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SmartX.Web/SmartX.Web.csproj SmartX.Web/
RUN dotnet restore SmartX.Web/SmartX.Web.csproj
COPY SmartX.Web/ SmartX.Web/
WORKDIR /src/SmartX.Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8081
ENV SmartXApi__BaseUrl=http://smartx-api:8080/
EXPOSE 8081
ENTRYPOINT ["dotnet", "SmartX.Web.dll"]
