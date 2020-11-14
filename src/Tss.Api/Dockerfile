#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Tss.Api/Tss.Api.csproj", "Tss.Api/"]
COPY ["Tss.Core/Tss.Core.csproj", "Tss.Core/"]
RUN dotnet restore "Tss.Api/Tss.Api.csproj"
COPY . .
WORKDIR "/src/Tss.Api"
RUN dotnet build "Tss.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tss.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tss.Api.dll"]
