FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Lash.Web/Lash.Web.csproj", "src/Lash.Web/"]
COPY ["src/Users/Lash.Users.Application/Lash.Users.Application.csproj", "src/Users/Lash.Users.Application/"]
COPY ["src/Users/Lash.Users.Domain/Lash.Users.Domain.csproj", "src/Users/Lash.Users.Domain/"]
COPY ["src/Users/Lash.Users.Infrastructure/Lash.Users.Infrastructure.csproj", "src/Users/Lash.Users.Infrastructure/"]
COPY ["src/Users/Lash.Users.Messaging/Lash.Users.Messaging.csproj", "src/Users/Lash.Users.Messaging/"]
COPY ["src/Users/Lash.Users.Presentation/Lash.Users.Presentation.csproj", "src/Users/Lash.Users.Presentation/"]
RUN dotnet restore "src/Lash.Web/Lash.Web.csproj"
COPY . .
WORKDIR "/src/src/Lash.Web"
RUN dotnet publish "Lash.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lash.Web.dll"]
