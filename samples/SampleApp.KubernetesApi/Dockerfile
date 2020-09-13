#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["samples/SampleApp.KubernetesApi/SampleApp.KubernetesApi.csproj", "samples/SampleApp.KubernetesApi/"]
COPY ["src/Akka.Cluster.Discovery.KubernetesApi/Akka.Cluster.Discovery.KubernetesApi.csproj", "src/Akka.Cluster.Discovery.KubernetesApi/"]
COPY ["src/Akka.Cluster.Discovery/Akka.Cluster.Discovery.csproj", "src/Akka.Cluster.Discovery/"]
COPY ["src/Akka.Cluster.Discovery.Consul/Akka.Cluster.Discovery.Consul.csproj", "src/Akka.Cluster.Discovery.Consul/"]
RUN dotnet restore "samples/SampleApp.KubernetesApi/SampleApp.KubernetesApi.csproj"
COPY . .
WORKDIR "/src/samples/SampleApp.KubernetesApi"
RUN dotnet build "SampleApp.KubernetesApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleApp.KubernetesApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleApp.KubernetesApi.dll"]