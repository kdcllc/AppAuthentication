#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the sample for hosting application inside of Kubernetes Cluster.
#
# NOTE:         Please run commands from root of the solution.
#
# BUILD:        docker build --rm -f "src/WorkerSample/Dockerfile" -t app:WorkerSample .
# 
# RUN:          docker run --rm -it  app:WorkerSample 
#
# NOTE:        See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#------------------------------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/WorkerSample/WorkerSample.csproj", "src/WorkerSample/"]
RUN dotnet restore "src/WorkerSample/WorkerSample.csproj"
COPY . .
WORKDIR "/src/src/WorkerSample"
RUN dotnet build "WorkerSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerSample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerSample.dll"]