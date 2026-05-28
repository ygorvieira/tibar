# Tibar - Plano de Desenvolvimento

## Stack Tecnológica

- **Backend**: .NET 10, ASP.NET Core, MediatR (CQRS), EF Core, FluentValidation
- **Banco**: PostgreSQL
- **Autenticação**: ASP.NET Core Identity + JWT
- **Frontend**: Angular (standalone components)
- **Testes**: XUnit + Moq
- **Infra**: Docker Compose (API + Postgres + Frontend)

## Estrutura da Solution

```
Tibar.sln
├── src/
│   ├── Tibar.Domain/           # Entidades, Value Objects, Enums ✅
│   ├── Tibar.Application/      # Commands, Queries, Handlers, DTOs, Interfaces ✅
│   ├── Tibar.Infrastructure/   # EF Core, Identity, Repositories, JWT ⬜
│   └── Tibar.API/              # Controllers, Middleware, Program.cs ⬜
├── tests/
│   └── Tibar.UnitTests/        # Testes unitários ⬜
└── frontend/                   # Angular standalone ⬜
```

## Etapas de Desenvolvimento

---

### Etapa 1 — Domain Layer ✅

**Status: Concluído**

- `BaseEntity` — classe abstrata com `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`
- `User` — Id, Name, Email, CreatedAt (herda de `BaseEntity`)
- `Category` — Id, Name, Type (Income/Expense), UserId (herda de `BaseEntity`)
- `Transaction` — Id, Description, Amount (Money), Type, CategoryId, UserId, Date (herda de `BaseEntity`)
- `Money` — Value Object imutável com validação de valor não negativo e moeda, operadores `+` e `-`
- `TransactionType` — enum `Income`, `Expense`
- `DomainException` — exceção base para regras de negócio

**Arquivos:**
```
src/Tibar.Domain/
├── Entities/
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Category.cs
│   └── Transaction.cs
├── Enums/
│   └── TransactionType.cs
├── ValueObjects/
│   └── Money.cs
└── Exceptions/
    └── DomainException.cs
```

---

### Etapa 2 — Application Layer (CQRS) ✅

**Status: Concluído**

- **Commands**: `CreateTransaction`, `UpdateTransaction`, `DeleteTransaction`, `CreateCategory`, `UpdateCategory`, `DeleteCategory`, `RegisterUser`, `LoginUser`
- **Queries**: `GetTransactionsByPeriod`, `GetBalanceByPeriod`, `GetCategories`
- **Handlers**: Implementados via MediatR, todos retornando `Result<T>`
- **DTOs**: `TransactionDto`, `CategoryDto`, `BalanceDto`, `RegisterRequest`, `LoginRequest`, `TokenResponse`
- **Validação**: FluentValidation nos commands de criação e auth
- **Interfaces**: `IAuthService`, `IApplicationDbContext`
- **Behaviors**: `ValidationBehavior` (validação automática via pipeline, retorna `Result.Failure`)
- **Result Pattern**: `Result<T>` com `IsValid`, `Errors[]`, `Data` — handlers nunca lançam exceptions, sempre retornam `Result`
- **DI**: `AddApplication()` registra MediatR + validators + behaviors

**Arquivos:**
```
src/Tibar.Application/
├── Commands/
│   ├── Transactions/
│   │   ├── CreateTransactionCommand.cs         (+ Handler, + Validator)
│   │   ├── UpdateTransactionCommand.cs         (+ Handler)
│   │   └── DeleteTransactionCommand.cs         (+ Handler)
│   ├── Categories/
│   │   ├── CreateCategoryCommand.cs            (+ Handler)
│   │   ├── UpdateCategoryCommand.cs            (+ Handler)
│   │   └── DeleteCategoryCommand.cs            (+ Handler)
│   └── Auth/
│       ├── RegisterUserCommand.cs              (+ Handler, + Validator)
│       └── LoginUserCommand.cs                 (+ Handler, + Validator)
├── Queries/
│   ├── Transactions/GetTransactionsByPeriodQuery.cs   (+ Handler)
│   ├── Categories/GetCategoriesQuery.cs               (+ Handler)
│   └── Dashboard/GetBalanceByPeriodQuery.cs           (+ Handler)
├── DTOs/
│   ├── TransactionDto.cs
│   ├── CategoryDto.cs
│   ├── BalanceDto.cs
│   └── Auth/
│       ├── RegisterRequest.cs
│       ├── LoginRequest.cs
│       └── TokenResponse.cs
├── Interfaces/
│   ├── IApplicationDbContext.cs
│   └── IAuthService.cs
├── Behaviors/
│   └── ValidationBehavior.cs
├── Common/
│   └── Result.cs
└── DependencyInjection.cs
```

---

### Etapa 3 — Infrastructure Layer ⬜

**Status: Pendente**

- [ ] `AppDbContext` com EF Core + Identity + PostgreSQL
- [ ] Configuração de entidades (Fluent API)
- [ ] Implementação de `IAuthService` com Identity + JWT
- [ ] Repositórios (se necessário)
- [ ] `Migrations` (EF Core)
- [ ] `appsettings.json` com connection string e JWT config

---

### Etapa 4 — API Layer ⬜

**Status: Pendente**

- [ ] Controllers REST:
  - `AuthController`: `POST /api/auth/register`, `POST /api/auth/login`
  - `TransactionsController`: CRUD com `[Authorize]`
  - `CategoriesController`: CRUD com `[Authorize]`
  - `DashboardController`: `GET /api/dashboard/balance`
- [ ] Filtro de transações por **período** (startDate/endDate) e **usuário logado**
- [ ] Exception handling middleware
- [ ] Swagger/OpenAPI

---

### Etapa 5 — Frontend: Setup Angular ⬜

**Status: Pendente**

- [ ] `ng new tibar-frontend` — standalone components, routing
- [ ] Models: `Transaction`, `Category`, `Balance`, `User`
- [ ] Serviços HTTP com `HttpClient`
- [ ] Interceptor de JWT
- [ ] Guards de autenticação
- [ ] Rotas: `/login`, `/register`, `/dashboard`, `/transactions`, `/categories`

---

### Etapa 6 — Frontend: Componentes ⬜

**Status: Pendente**

- [ ] **LoginComponent / RegisterComponent**: Formulários de autenticação
- [ ] **DashboardComponent**: Saldo do período (receitas - despesas)
- [ ] **TransactionListComponent**: Lista com filtro por data, CRUD
- [ ] **TransactionFormComponent**: Cadastro/edição com seleção de categoria
- [ ] **CategoryListComponent**: Gerenciamento de categorias
- [ ] **NavbarComponent**: Navegação com estado do usuário

---

### Etapa 7 — Frontend: Integração ⬜

**Status: Pendente**

- [ ] Consumo de `AuthController` (login/register)
- [ ] Consumo de `TransactionsController`, `CategoriesController`, `DashboardController`
- [ ] Tratamento de erros global (HttpInterceptor)
- [ ] Loading states e feedback visual

---

### Etapa 8 — Testes Unitários (XUnit) ⬜

**Status: Pendente**

- [ ] **Domain**: regras de negócio (Money, Transaction)
- [ ] **Application**: handlers (CreateTransactionHandler, GetBalanceQueryHandler)
- [ ] **API**: controllers mockados

---

### Etapa 9 — Docker e Infra ⬜

**Status: Pendente**

- [ ] `docker-compose.yml`:
  ```yaml
  services:
    api:     # Tibar.API
    db:      # postgres:16
    frontend: # nginx servindo Angular build
  ```
- [ ] `Dockerfile` para API (multi-stage build .NET 10)
- [ ] `Dockerfile` para Frontend (multi-stage build Node + Nginx)
- [ ] Script de seed: categorias padrão (Alimentação, Transporte, Moradia, Lazer, Saúde, Educação, etc.)
- [ ] README com instruções de execução

---

## Backlog (para versões futuras)

- Orçamentos mensais por categoria
- Relatórios e gráficos
- Subcategorias
- Alertas de limite
- Múltiplas contas/carteiras
