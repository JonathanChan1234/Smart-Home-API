FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /Smart-Home-API

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 5181
EXPOSE 1883
WORKDIR /Smart-Home-API
COPY --from=build-env /Smart-Home-API/out .
ENTRYPOINT ["dotnet", "smart_home_server.dll"]