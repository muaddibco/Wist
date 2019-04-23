FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Node/Wist.Node/Wist.Node.csproj Node/Wist.Node/
COPY Node/Wist.Node.Core/Wist.Node.Core.csproj Node/Wist.Node.Core/
COPY ../Wist.Common/Network/Wist.Network/Wist.Network.csproj ../Wist.Common/Network/Wist.Network/
COPY ../Wist.Common/Blockchain/Wist.Blockchain.Core/Wist.Blockchain.Core.csproj ../Wist.Common/Blockchain/Wist.Blockchain.Core/
COPY ../Wist.Common/Crypto/Wist.Crypto/Wist.Crypto.csproj ../Wist.Common/Crypto/Wist.Crypto/
COPY ../Wist.Common/Crypto/HashLib/CoreHashLib/CoreHashLib.csproj ../Wist.Common/Crypto/HashLib/CoreHashLib/
COPY ../Wist.Common/Crypto/Chaos.NaCl/Chaos.NaCl.csproj ../Wist.Common/Crypto/Chaos.NaCl/
COPY ../Wist.Common/Wist.Core/Wist.Core.csproj ../Wist.Common/Wist.Core/
COPY ../Wist.Common/Network/Wist.Proto/Wist.Proto.csproj ../Wist.Common/Network/Wist.Proto/
COPY ../Wist.Common/Blockchain/Wist.Blockchain.DataModel/Wist.Blockchain.DataModel.csproj ../Wist.Common/Blockchain/Wist.Blockchain.DataModel/
COPY ../Wist.Common/Blockchain/Wist.Blockchain.SQLite/Wist.Blockchain.SQLite.csproj ../Wist.Common/Blockchain/Wist.Blockchain.SQLite/
RUN dotnet restore Node/Wist.Node/Wist.Node.csproj
COPY . .
WORKDIR /src/Node/Wist.Node
RUN dotnet build Wist.Node.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Wist.Node.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Wist.Node.dll"]
