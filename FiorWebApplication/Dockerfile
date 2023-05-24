FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FiorWebApplication/FiorWebApplication.csproj", "FiorWebApplication/"]
RUN dotnet restore "FiorWebApplication/FiorWebApplication.csproj"
COPY . .
WORKDIR "/src/FiorWebApplication"
RUN dotnet build "FiorWebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FiorWebApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FiorWebApplication.dll"]
