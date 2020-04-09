FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

COPY . /app
WORKDIR /app/Samples/Samples.Db.WebApi
RUN dotnet publish Samples.Db.WebApi.csproj -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/Samples/Samples.Db.WebApi/out .

EXPOSE 80
HEALTHCHECK --interval=5s --timeout=3s --retries=3 CMD curl -f / http://localhost:80/health-check || exit 1 
ENTRYPOINT ["dotnet", "Samples.Db.WebApi.dll"]