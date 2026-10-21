FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TunetalkApi.Api.slnx
RUN dotnet publish TunetalkApi.Api/TunetalkApi.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ARG APP_UID=1654
WORKDIR /app
COPY --from=build --chown=${APP_UID}:${APP_UID} /app/publish .
RUN mkdir -p /data && chown ${APP_UID}:${APP_UID} /data
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ConnectionStrings__DefaultConnection="Data Source=/data/tunetalk.db"
EXPOSE 8080
USER ${APP_UID}
ENTRYPOINT ["dotnet", "TunetalkApi.Api.dll"]
