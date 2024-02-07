# Build environment with .NET SDK and CUDA
FROM nvidia/cuda:12.3.1-devel-ubuntu20.04 AS build-env
WORKDIR /App

# Install tzdata and set the timezone
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y tzdata \
    && echo "Europe/Paris" > /etc/timezone \
    && dpkg-reconfigure -f noninteractive tzdata

# Install .NET SDK
RUN apt-get install -y wget \
    && wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-8.0

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image with ASP.NET and CUDA
FROM nvidia/cuda:12.3.1-devel-ubuntu20.04
WORKDIR /App

# Install tzdata and set the timezone
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y tzdata \
    && echo "Europe/Paris" > /etc/timezone \
    && dpkg-reconfigure -f noninteractive tzdata

# Install ASP.NET Runtime
RUN apt-get install -y wget \
    && wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y aspnetcore-runtime-8.0

COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]
