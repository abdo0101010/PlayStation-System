# FIRST STAGE BASE IMAGE
FROM  mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
#COPY CSPROJ AND RESTORE DEPENDENCIES
COPY *csproj .
RUN dotnet restore
#COPY AND PUBLISH PROJECT FILE
RUN dotnet publish -c Release -o /app


#FINAL STAGE BASE IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "PlayStation-System.dll"]