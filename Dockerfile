FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV NUGET_HTTP_TIMEOUT=600
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

COPY ["NuGet.config", "./"]
COPY ["Atlantis Portfolio-Backend.csproj", "./"]
COPY ["Atlantis Portfolio-Backend-BLL/Atlantis Portfolio-Backend-BLL.csproj", "Atlantis Portfolio-Backend-BLL/"]
COPY ["Atlantis Portfolio-Backend-DAL/Atlantis Portfolio-Backend-DAL.csproj", "Atlantis Portfolio-Backend-DAL/"]

RUN dotnet restore "Atlantis Portfolio-Backend.csproj" --disable-parallel

COPY . .
RUN dotnet publish "Atlantis Portfolio-Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Atlantis Portfolio-Backend.dll"]