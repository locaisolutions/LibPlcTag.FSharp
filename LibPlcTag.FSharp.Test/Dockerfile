FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LibPlcTag.FSharp.Test/LibPlcTag.FSharp.Test.fsproj", "LibPlcTag.FSharp.Test/"]
RUN dotnet restore "LibPlcTag.FSharp.Test/LibPlcTag.FSharp.Test.fsproj"
COPY . .
WORKDIR "/src/LibPlcTag.FSharp.Test"
RUN dotnet build "LibPlcTag.FSharp.Test.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibPlcTag.FSharp.Test.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibPlcTag.FSharp.Test.dll"]
