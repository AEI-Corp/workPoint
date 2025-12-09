FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["workpoint.Api/workpoint.Api.csproj", "workpoint.Api/"]
COPY ["workpoint.Application/workpoint.Application.csproj", "workpoint.Application/"]
COPY ["workpoint.Domain/workpoint.Domain.csproj", "workpoint.Domain/"]
COPY ["workpoint.Infrastructure/workpoint.Infrastructure.csproj", "workpoint.Infrastructure/"]

RUN dotnet restore "workpoint.Api/workpoint.Api.csproj"

COPY . .

WORKDIR /src/workpoint.Api

RUN dotnet build "workpoint.Api.csproj" -c Release -o /app/build

FROM build AS publish

WORKDIR /src/workpoint.Api

RUN dotnet publish "workpoint.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 5001

ENTRYPOINT ["dotnet", "workpoint.Api.dll"]