FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VLO-ACCOUNTS/VLO-ACCOUNTS.csproj", "VLO-ACCOUNTS/"]
COPY ["AccountsData/AccountsData.csproj", "AccountsData/"]
RUN dotnet restore "VLO-ACCOUNTS/VLO-ACCOUNTS.csproj"
COPY . .
WORKDIR "/src/VLO-ACCOUNTS"
RUN dotnet build "VLO-ACCOUNTS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VLO-ACCOUNTS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VLO-ACCOUNTS.dll"]
