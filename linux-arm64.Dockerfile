# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0-cbl-mariner2.0-arm64v8 AS build
WORKDIR /source
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app -r linux-arm64 --self-contained true /p:PublishTrimmed=true

# Stage 2: Build the runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0-cbl-mariner2.0-distroless-arm64v8
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["/app/jobs-dotnet"]
CMD ["hello"]
