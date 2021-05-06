# Build the APP
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11 as buildenv  
WORKDIR /root/src/app

ARG project_name

COPY ./Wolf.MessageQueue/Wolf.MessageQueue.csproj ./Wolf.MessageQueue/Wolf.MessageQueue.csproj
RUN dotnet restore ./Wolf.MessageQueue/Wolf.MessageQueue.csproj

COPY ./${project_name}/${project_name}.csproj ./${project_name}/${project_name}.csproj
RUN dotnet restore ./${project_name}/${project_name}.csproj 

COPY ./Wolf.MessageQueue ./Wolf.MessageQueue
COPY ./${project_name} ./${project_name}

RUN dotnet publish ./${project_name}/${project_name}.csproj -c debug -o /root/src/app/published --self-contained -r alpine-x64
COPY ./${project_name}/appsettings.production.json /root/src/app/published/appsettings.json

# Run APP
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.2-alpine3.11
WORKDIR /usr/local/app

COPY --from=buildenv /root/src/app/published .

RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080/tcp

CMD ["dotnet", "/usr/local/app/${project_name}.dll"]