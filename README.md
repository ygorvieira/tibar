# Tibar

Sistema de finanças pessoais com **.NET 10 + Angular 21** — backend CQRS, autenticação JWT e PostgreSQL.

## Stack

| Camada | Tecnologia |
|--------|------------|
| API | ASP.NET Core 10, MediatR (CQRS), FluentValidation |
| ORM | EF Core 10 + Npgsql |
| Auth | ASP.NET Core Identity + JWT |
| Banco | PostgreSQL 16+ |
| Frontend | Angular 21 (standalone, signals, lazy-loading) |
| Testes | XUnit + Moq |

## Estrutura

```
Tibar.slnx
├── src/
│   ├── Tibar.Domain/           # Entidades, Value Objects, Enums
│   ├── Tibar.Application/      # Commands, Queries, Handlers, DTOs, Interfaces
│   ├── Tibar.Infrastructure/   # EF Core, Identity, JWT
│   └── Tibar.API/              # Controllers, Middleware, Program.cs
├── tests/
│   └── Tibar.UnitTests/        # Testes unitários (42 testes)
└── frontend/                   # Angular 21 standalone
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Node.js 22+](https://nodejs.org/)
- Angular CLI: `npm install -g @angular/cli`

## Setup

### 1. Banco de dados

```bash
# Crie o banco PostgreSQL
createdb tibar

# Ou via Docker
docker run -d --name tibar-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=tibar -p 5432:5432 postgres:16
```

### 2. Backend

```bash
dotnet restore
dotnet build --no-restore

# Migration (cria as tabelas)
dotnet ef database update -p src/Tibar.Infrastructure -s src/Tibar.API

# Rodar
dotnet run --project src/Tibar.API
```

API disponível em `https://localhost:5001` | Swagger em `https://localhost:5001/swagger`.

### 3. Frontend

```bash
cd frontend
npm install
ng serve
```

Frontend disponível em `http://localhost:4200`.

## Testes

```bash
dotnet test tests/Tibar.UnitTests
```

**42 testes** — Domain (regras de negócio), Application (handlers), API (controllers mockados).

## API

### Auth
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/auth/register` | Registrar novo usuário |
| POST | `/api/auth/login` | Login (retorna JWT) |

### Transactions (autenticado)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/transactions?startDate=&endDate=` | Listar por período |
| POST | `/api/transactions` | Criar transação |
| PUT | `/api/transactions/{id}` | Atualizar |
| DELETE | `/api/transactions/{id}` | Remover |

### Categories (autenticado)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/categories` | Listar |
| POST | `/api/categories` | Criar |
| PUT | `/api/categories/{id}` | Atualizar |
| DELETE | `/api/categories/{id}` | Remover |

### Dashboard (autenticado)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/dashboard/balance?startDate=&endDate=` | Saldo do período |

## Rotas do Frontend

| Rota | Componente |
|------|------------|
| `/` | Dashboard |
| `/transactions` | Lista de transações |
| `/categories` | Gerenciar categorias |
| `/login` | Login |
| `/register` | Cadastro |

## Docker

```bash
# Iniciar todos os serviços
docker compose up --build

# Apenas o banco de dados
docker compose up db -d
```

Serviços:
| Serviço | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| API | http://localhost:5000/api |
| Swagger | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |

Na primeira execução, o seed cria automaticamente:
- **Admin**: `admin@tibar.com` / `Admin@123`
- **12 categorias padrão**: Salário, Freelance, Investimentos, Outros, Alimentação, Transporte, Moradia, Lazer, Saúde, Educação, Assinaturas, Compras

## Status

- [x] Domain Layer — entidades, value objects, enums
- [x] Application Layer — CQRS, handlers, validação, Result pattern
- [x] Infrastructure Layer — EF Core, Identity, JWT, PostgreSQL
- [x] API Layer — controllers, middleware, Swagger
- [x] Frontend Setup — Angular standalone, lazy routes
- [x] Frontend Components — nav, login, register, dashboard, transactions, categories
- [x] Frontend Integration — HTTP services, interceptors, loading/error states
- [x] Unit Tests — 42 testes (Domain + Application + API)
- [x] Docker Compose — API + Postgres + Frontend
- [x] Seed de categorias padrão
