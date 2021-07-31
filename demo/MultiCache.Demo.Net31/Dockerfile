FROM mcr.microsoft.com/dotnet/sdk:3.1-bionic AS build-env
ENV PROJECT_NAME="MultiCache.Demo.Net31"
WORKDIR /app
COPY . ./

#RUN dotnet test
RUN dotnet restore $PROJECT_NAME.csproj \
  && dotnet publish $PROJECT_NAME.csproj -c Release -o /app/out 
  
FROM mcr.microsoft.com/dotnet/aspnet:3.1-bionic

WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "MultiCache.Demo.Net31.dll"]
