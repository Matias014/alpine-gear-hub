# AlpineGearHub

A niche C2C marketplace for climbing and mountaineering gear — safety-aware listings, real-time buyer↔seller chat, community moderation, and paid listing promotions.

![CI](https://github.com/Matias014/alpine-gear-hub/actions/workflows/ci.yml/badge.svg)

## Stack

| Layer    | Technology |
|----------|-----------|
| Frontend | React 19, TypeScript, Vite, Tailwind CSS 4, TanStack Query 5, React Hook Form, Zod |
| Backend  | .NET 10, ASP.NET Core Web API, EF Core 10, MediatR, FluentValidation, SignalR |
| Database | PostgreSQL 18 |
| Cache    | Redis |
| Storage  | MinIO (S3-compatible, local) / AWS S3 (production) |
| Payments | Stripe (test mode) |
| Auth     | JWT Bearer tokens + refresh tokens, ASP.NET Core Identity (password hashing) |
| Tests    | xUnit + Testcontainers (backend), Vitest + Testing Library (frontend) |
| CI       | GitHub Actions |

## Architecture

The backend is a **modular monolith** — one deployable process, but the codebase is sliced vertically by business capability. Each module (`Identity`, `Listings`, `Chat`, `Moderation`, `Promotions`) is internally structured as **Clean Architecture** with Domain / Application / Infrastructure layers. Modules communicate exclusively through domain events and explicit contracts — never by referencing each other's infrastructure or domain layer directly.

Each module owns a separate PostgreSQL schema (`identity`, `listings`, `chat`, `moderation`, `promotions`), enforcing the modular boundary at the database level without requiring a separate database per module.

Design principles applied throughout: **DDD** (aggregate roots, value objects, domain events), **SOLID** (one handler per command/query, interface-driven dependencies, no god-services).

## Features

- **Listings** — create, edit, and browse gear for sale with category, condition, price, location, manufacture date, and safety certification fields (e.g. CE EN 892, UIAA 101)
- **Listing state machine** — `Draft → Active → Reserved → Sold` / `Active → Expired → Active` (renew) / `Active → Removed` (moderation only)
- **Categories** — Ropes, Harnesses, Helmets, Crampons, Ice Axes, Carabiners, Backpacks, Tents, Boots
- **Search & filtering** — filter by category, price range, condition, location; full-text search; pagination
- **Image upload** — up to 8 photos per listing, stored in MinIO (local) or S3 (production)
- **Real-time chat** — buyer↔seller messaging via SignalR, with full message history
- **Roles** — Member (list and buy), Moderator (act on reports), Admin (full access — categories, users)
- **Reports** — flag listings for Counterfeit, Prohibited, Scam, SafetyConcern, or Other; moderation workflow for Moderators and Admins
- **Promotions** — pay to boost a listing to the top of search results (Stripe test mode)
- **Redis cache** — category list, popular searches, login rate-limiting

## Getting started

### Option A — Docker (recommended)

The entire stack runs with a single command. Only [Docker](https://docs.docker.com/get-docker/) is required.

```bash
git clone https://github.com/Matias014/alpine-gear-hub.git
cd alpine-gear-hub
docker compose up --build
```

| Service   | URL                            | Credentials |
|-----------|--------------------------------|-------------|
| App       | http://localhost:3000          | see below |
| API       | http://localhost:8080          | — |
| Swagger   | http://localhost:8080/swagger  | — |
| MinIO     | http://localhost:9001          | user: `minioadmin` / pass: `minioadmin` |
| pgAdmin   | http://localhost:5050          | email: `admin@alpinegearhub.local` / pass: `admin` |

Migrations and the seed admin account are applied automatically on first start.

**Default admin credentials**

| Field    | Value |
|----------|-------|
| Email    | `admin@alpinegearhub.local` |
| Password | `Admin1234!` |

To override secrets (JWT key, Stripe test key, etc.):

```bash
cp .env.example .env   # then edit .env
docker compose up --build
```

---

### Option B — Manual setup (for development)

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 22+](https://nodejs.org/), [Docker](https://docs.docker.com/get-docker/) (for PostgreSQL, Redis, MinIO)

**1 — Start infrastructure**

```bash
docker compose up -d postgres redis minio
```

**2 — Run the backend**

```bash
cd backend
dotnet run --project src/Host/AlpineGearHub.Api
```

The API starts on **http://localhost:8080**. Migrations and the default admin account are applied automatically.  
Swagger UI is available at **http://localhost:8080/swagger**.

**3 — Run the frontend**

```bash
cd frontend
npm install
npm run dev
```

The app opens at **http://localhost:5173**.

## Running tests

**Backend** (integration tests, requires Docker):

```bash
cd backend
dotnet test
```

**Frontend** (unit tests):

```bash
cd frontend
npm test
```

## Database schema

```mermaid
erDiagram
    USERS {
        uuid id PK
        varchar email
        varchar password_hash
        varchar full_name
        varchar role
        timestamp created_at
    }
    CATEGORIES {
        uuid id PK
        varchar name
        varchar slug
    }
    LISTINGS {
        uuid id PK
        varchar title
        text description
        uuid category_id FK
        uuid seller_id FK
        numeric price
        varchar currency
        varchar condition
        varchar status
        date manufacture_date
        varchar safety_certification
        varchar location
        int views_count
        boolean is_promoted
        timestamp created_at
        timestamp updated_at
        timestamp expires_at
    }
    LISTING_IMAGES {
        uuid id PK
        uuid listing_id FK
        varchar storage_key
        int sort_order
        boolean is_primary
    }
    CONVERSATIONS {
        uuid id PK
        uuid listing_id FK
        uuid buyer_id FK
        uuid seller_id FK
        timestamp created_at
        timestamp last_message_at
    }
    MESSAGES {
        uuid id PK
        uuid conversation_id FK
        uuid sender_id FK
        text body
        timestamp sent_at
        timestamp read_at
    }
    REPORTS {
        uuid id PK
        uuid listing_id FK
        uuid reported_by_user_id FK
        varchar reason
        text description
        varchar status
        uuid reviewed_by_user_id FK
        timestamp reviewed_at
    }
    PROMOTIONS {
        uuid id PK
        uuid listing_id FK
        varchar tier
        timestamp start_at
        timestamp end_at
        int price_cents
        varchar currency
        varchar payment_status
        varchar stripe_payment_intent_id
    }

    USERS ||--o{ LISTINGS : "sells"
    USERS ||--o{ CONVERSATIONS : "buys in"
    USERS ||--o{ CONVERSATIONS : "sells in"
    USERS ||--o{ MESSAGES : "sends"
    USERS ||--o{ REPORTS : "files"
    CATEGORIES ||--o{ LISTINGS : "categorises"
    LISTINGS ||--o{ LISTING_IMAGES : "has"
    LISTINGS ||--o{ CONVERSATIONS : "discussed in"
    LISTINGS ||--o{ REPORTS : "reported via"
    LISTINGS ||--o{ PROMOTIONS : "boosted by"
    CONVERSATIONS ||--o{ MESSAGES : "contains"
```
