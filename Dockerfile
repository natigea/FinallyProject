FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY EcommersProject.slnx ./
COPY EcommersProject/EcommersProject.csproj EcommersProject/
COPY EcommersProject.BLL/EcommersProject.BLL.csproj EcommersProject.BLL/
COPY EcommersProject.DAL/EcommersProject.DAL.csproj EcommersProject.DAL/
COPY EcommersProject.API/EcommersProject.API.csproj EcommersProject.API/
RUN dotnet restore EcommersProject/EcommersProject.csproj

COPY . .
RUN dotnet publish EcommersProject/EcommersProject.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EcommersProject.dll"]
