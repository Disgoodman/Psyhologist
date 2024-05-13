FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Docker -o /app


FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
ENV DOTNET_URLS="http://*:8080"
ENV DB="Host=db;Port=5432;Username=postgres;Password=123;Database=psychologist"
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Psychologist.Server.dll"]

# docker build -t psychologist .
# docker run -p 8080:8080 -p 5432:5432 --rm psychologist