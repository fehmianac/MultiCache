FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
ENV PROJECT_NAME="MultiCache.Demo.Net50"
WORKDIR /app
COPY . ./

#RUN dotnet test
RUN dotnet restore $PROJECT_NAME.csproj \
  && dotnet publish $PROJECT_NAME.csproj -c Release -o /app/out 
  
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim

WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "MultiCache.Demo.Net50.dll"]
