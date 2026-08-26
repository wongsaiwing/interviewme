FROM node:22-alpine AS web
WORKDIR /repo
COPY frontend/InterviewMe.Web/package.json frontend/InterviewMe.Web/
WORKDIR /repo/frontend/InterviewMe.Web
RUN npm install
COPY frontend/InterviewMe.Web/ ./
RUN mkdir -p /repo/src/InterviewMe.Api/wwwroot
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY InterviewMe.sln ./
COPY src ./src
COPY tests ./tests
COPY knowledge ./knowledge
COPY --from=web /repo/src/InterviewMe.Api/wwwroot ./src/InterviewMe.Api/wwwroot
RUN dotnet restore InterviewMe.sln
RUN dotnet publish src/InterviewMe.Api/InterviewMe.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./
COPY knowledge ./knowledge
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Knowledge__Path=/app/knowledge
EXPOSE 8080
CMD ["dotnet", "InterviewMe.Api.dll"]
