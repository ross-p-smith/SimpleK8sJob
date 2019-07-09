FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["SimpleJob/SimpleJob.csproj", "SimpleJob/"]
RUN dotnet restore "SimpleJob/SimpleJob.csproj"
COPY . .
WORKDIR "/src/SimpleJob"
RUN dotnet build "SimpleJob.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleJob.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleJob.dll"]