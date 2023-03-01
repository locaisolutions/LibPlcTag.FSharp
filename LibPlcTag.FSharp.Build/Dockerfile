FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LibPlcTag.FSharp.Build/LibPlcTag.FSharp.Build.fsproj", "LibPlcTag.FSharp.Build/"]
RUN dotnet restore "LibPlcTag.FSharp.Build/LibPlcTag.FSharp.Build.fsproj"
COPY . .
WORKDIR "/src/LibPlcTag.FSharp.Build"
RUN dotnet build "LibPlcTag.FSharp.Build.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibPlcTag.FSharp.Build.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibPlcTag.FSharp.Build.dll"]
