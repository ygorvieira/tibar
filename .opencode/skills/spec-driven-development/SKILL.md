---
name: spec-driven-development
description: >
  Use when working with API-first / spec-driven development workflow in the
  Tibar project. Trigger keywords: spec-first, OpenAPI, Swagger, contrato,
  gerar cliente, API client, contrato primeiro, spec driven.
  Use ONLY for this project's .NET + Angular stack.
---

# Spec-Driven Development — Tibar

Este skill define o fluxo de trabalho **Spec-Driven Development (SDD)** para o
projeto Tibar. A especificação OpenAPI é a fonte da verdade para o contrato
entre backend (C# .NET) e frontend (Angular).

## Workflow

Toda mudança na API **começa pela spec**, nunca pelo código.

```
1. Editar spec/openapi.json  (contrato)
2. Validar spec com backend
3. Implementar handlers e controllers
4. Gerar cliente TypeScript para o frontend
5. Verificar que os testes passam
```

---

## 1. Estrutura do projeto

```
spec/openapi.json              ← spec versionada (fonte da verdade)
src/Tibar.API/Controllers/     ← controllers (4 arquivos)
src/Tibar.Application/DTOs/    ← DTOs compartilhados
src/Tibar.Application/Commands/ ← CQRS commands + handlers
src/Tibar.Application/Queries/  ← CQRS queries + handlers
frontend/src/app/models/       ← modelos TS manuais (substituir por gerados)
frontend/src/app/services/     ← services HTTP manuais (substituir por gerados)
```

## 2. Gerar/atualizar a spec OpenAPI

Com o backend rodando:

```bash
# Via CLI do Swashbuckle
dotnet tool install --global Swashbuckle.AspNetCore.Cli
dotnet swagger tofile --output spec/openapi.json \
  src/Tibar.API/bin/Debug/net10.0/Tibar.API.dll v1
```

Ou via HTTP (ambiente dev):

```bash
curl -o spec/openapi.json https://localhost:5001/swagger/v1/swagger.json
```

Commite a spec atualizada no repositório.

## 3. Anotar controllers para spec rica

Para que a spec gerada seja completa, todo controller endpoint DEVE ter:

- `[ProducesResponseType(typeof(T), StatusCodes.Status200OK)]`
- `[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]`
- XML comments nos DTOs e commands quando o tipo não for autoexplicativo

O `.csproj` da API já deve ter:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

E no `Program.cs`:

```csharp
c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Tibar.API.xml"));
```

## 4. Implementar spec-first

Ao adicionar/modificar um endpoint:

1. **Edite** `spec/openapi.json` primeiro — adicione o path, schema, parâmetros
2. **Crie/altere** o Command ou Query em `src/Tibar.Application/Commands/` ou `Queries/`
3. **Crie/altere** o Handler correspondente
4. **Crie/altere** o Validator (FluentValidation) no mesmo diretório
5. **Crie/altere** o endpoint no Controller
6. **Adicione** testes unitários no projeto `tests/Tibar.UnitTests/`
7. **Regenere** a spec e verifique que `git diff spec/openapi.json` só mostra o que foi planejado

## 5. Gerar cliente Angular

Após atualizar a spec, gere o service layer do frontend automaticamente:

```bash
# Com NSwag
npx nswag openapi2tsclient \
  /input:spec/openapi.json \
  /output:frontend/src/app/generated/api.ts \
  /Template:Angular \
  /RxJsVersion:7.8 \
  /HttpClass:HttpClient \
  /UseSingletonProvider:true

# Ou com openapi-generator
npx @openapitools/openapi-generator-cli generate \
  -i spec/openapi.json \
  -g typescript-angular \
  -o frontend/src/app/generated
```

Depois de gerar:

1. Remova ou adapte os modelos manuais em `frontend/src/app/models/`
2. Remova ou adapte os services manuais em `frontend/src/app/services/`
3. Ajuste o `error.interceptor.ts` para funcionar com o cliente gerado
4. Atualize os componentes que usam os services antigos

## 6. Contrato de erro

O formato de erro da API é consistente:

```json
{
  "errors": ["mensagem 1", "mensagem 2"]
}
```

- Erros de validação (FluentValidation) retornam `400 Bad Request`
- Erros de domínio (DomainException) retornam o status code da exceção
- Erros inesperados retornam `500` com `"Ocorreu um erro inesperado."`

O cliente gerado DEVE usar um `ErrorInterceptor` que trata esse formato.

## 7. CI / validação

Adicione ao pipeline de CI:

1. Gerar a spec: `dotnet swagger tofile ...`
2. Validar que a spec está versionada: `git diff --exit-code spec/openapi.json`
3. Validar breaking changes com `openapi-diff` ou similar
4. Gerar cliente TypeScript e verificar se a compilação Angular passa
5. Rodar `dotnet test`

## 8. Convenções do projeto

- Controllers usam `[ApiController]`, `[Route("api/[controller]")]`
- Result pattern: toda action verifica `result.IsValid` antes de retornar
- UserId é extraído do JWT via `GetUserId()` e sobrescrito no command
- Commands usam `record` types com `with` expression para sobrescrever UserId
- Validação é feita com FluentValidation + pipeline behavior do MediatR

## 9. Referências

- Spec: `spec/openapi.json`
- Controllers: `src/Tibar.API/Controllers/`
- Application: `src/Tibar.Application/`
- Domain: `src/Tibar.Domain/`
- Testes: `tests/Tibar.UnitTests/`
- Frontend: `frontend/src/app/`
