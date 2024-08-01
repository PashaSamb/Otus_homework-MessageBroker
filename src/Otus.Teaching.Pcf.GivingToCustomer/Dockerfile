FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Add these lines for debugging
RUN apt-get update && apt-get install -y iputils-ping
RUN ping -c 4 api.nuget.org
RUN cat /etc/resolv.conf

# Copy NuGet.config if you have one
COPY NuGet.config .

# copy csproj and restore as distinct layers
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done

# Modify the restore command to output more information
RUN dotnet restore --verbosity detailed --configfile NuGet.config || (echo "Restore failed" && exit 1)

# copy everything else and build app
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
ENTRYPOINT ["dotnet", "Otus.Teaching.Pcf.GivingToCustomer.WebHost.dll"]