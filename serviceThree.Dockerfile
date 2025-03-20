FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App
EXPOSE 5003


# Copy everything
COPY ./service3 ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build /App/out ./

ENV ASPNETCORE_HTTP_PORTS=5003

ENTRYPOINT ["dotnet", "bff.dll"]