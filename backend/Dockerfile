# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SharedKernel/AlpineGearHub.SharedKernel/AlpineGearHub.SharedKernel.csproj                                       src/SharedKernel/AlpineGearHub.SharedKernel/
COPY src/Modules/Identity/AlpineGearHub.Identity.Domain/AlpineGearHub.Identity.Domain.csproj                             src/Modules/Identity/AlpineGearHub.Identity.Domain/
COPY src/Modules/Identity/AlpineGearHub.Identity.Application/AlpineGearHub.Identity.Application.csproj                   src/Modules/Identity/AlpineGearHub.Identity.Application/
COPY src/Modules/Identity/AlpineGearHub.Identity.Infrastructure/AlpineGearHub.Identity.Infrastructure.csproj             src/Modules/Identity/AlpineGearHub.Identity.Infrastructure/
COPY src/Modules/Listings/AlpineGearHub.Listings.Domain/AlpineGearHub.Listings.Domain.csproj                             src/Modules/Listings/AlpineGearHub.Listings.Domain/
COPY src/Modules/Listings/AlpineGearHub.Listings.Application/AlpineGearHub.Listings.Application.csproj                   src/Modules/Listings/AlpineGearHub.Listings.Application/
COPY src/Modules/Listings/AlpineGearHub.Listings.Infrastructure/AlpineGearHub.Listings.Infrastructure.csproj             src/Modules/Listings/AlpineGearHub.Listings.Infrastructure/
COPY src/Modules/Chat/AlpineGearHub.Chat.Domain/AlpineGearHub.Chat.Domain.csproj                                         src/Modules/Chat/AlpineGearHub.Chat.Domain/
COPY src/Modules/Chat/AlpineGearHub.Chat.Application/AlpineGearHub.Chat.Application.csproj                               src/Modules/Chat/AlpineGearHub.Chat.Application/
COPY src/Modules/Chat/AlpineGearHub.Chat.Infrastructure/AlpineGearHub.Chat.Infrastructure.csproj                         src/Modules/Chat/AlpineGearHub.Chat.Infrastructure/
COPY src/Modules/Moderation/AlpineGearHub.Moderation.Domain/AlpineGearHub.Moderation.Domain.csproj                       src/Modules/Moderation/AlpineGearHub.Moderation.Domain/
COPY src/Modules/Moderation/AlpineGearHub.Moderation.Application/AlpineGearHub.Moderation.Application.csproj             src/Modules/Moderation/AlpineGearHub.Moderation.Application/
COPY src/Modules/Moderation/AlpineGearHub.Moderation.Infrastructure/AlpineGearHub.Moderation.Infrastructure.csproj       src/Modules/Moderation/AlpineGearHub.Moderation.Infrastructure/
COPY src/Modules/Promotions/AlpineGearHub.Promotions.Domain/AlpineGearHub.Promotions.Domain.csproj                       src/Modules/Promotions/AlpineGearHub.Promotions.Domain/
COPY src/Modules/Promotions/AlpineGearHub.Promotions.Application/AlpineGearHub.Promotions.Application.csproj             src/Modules/Promotions/AlpineGearHub.Promotions.Application/
COPY src/Modules/Promotions/AlpineGearHub.Promotions.Infrastructure/AlpineGearHub.Promotions.Infrastructure.csproj       src/Modules/Promotions/AlpineGearHub.Promotions.Infrastructure/
COPY src/Host/AlpineGearHub.Api/AlpineGearHub.Api.csproj                                                                 src/Host/AlpineGearHub.Api/

RUN dotnet restore src/Host/AlpineGearHub.Api/AlpineGearHub.Api.csproj

COPY . .

RUN dotnet publish src/Host/AlpineGearHub.Api/AlpineGearHub.Api.csproj \
    -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AlpineGearHub.Api.dll"]
