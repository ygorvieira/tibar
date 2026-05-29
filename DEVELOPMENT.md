# Fluxo de Desenvolvimento

## Git Flow

- **Nunca** commitar diretamente na `main`.
- **Toda e qualquer alteração no código** (features, fixes, refactors, chores, docs, testes) **deve começar com uma branch limpa a partir da `main`**.
- **Nunca reaproveitar uma branch existente** — cada alteração tem sua própria branch.
- Prefixos obrigatórios:
  - `fix/` para correções
  - `feat/` para novas funcionalidades
  - `chore/` para tarefas de manutenção
- Abrir Pull Request e fazer merge para `main` após revisão.

```
git checkout main
git pull origin main
git checkout -b feat/nome-da-feature
git commit -m "feat: descrição"
git push origin feat/nome-da-feature
```

## Commits

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>: <descrição>

tipos: feat, fix, refactor, chore, docs, test, style
```

Exemplos:
```
feat: add transaction export to CSV
fix: handle null category on transaction list
refactor: extract date formatting helper
```

## Setup

1. Copie `.env.example` para `.env` e preencha as variáveis.
2. Suba os containers:
   ```bash
   docker compose up -d
   ```
3. As migrations e seed do admin (`admin@tibar.com` / `Admin@123`) rodam automaticamente na inicialização da API.

## GitHub Secrets

Adicione no repositório (`Settings > Secrets and variables > Actions`):

| Secret | Descrição |
|---|---|
| `GOOGLE_API_KEY` | Chave do Google AI Studio (gemini-2.0-flash gratuito) |
| `ANTHROPIC_API_KEY` | Chave da Anthropic para revisão automática de PRs via opencode |

## Testes

```bash
dotnet test
```

Sempre execute os testes antes de abrir um PR.

## Code Style

- Sempre seguir os padrões existentes no código.
- Sem comentários desnecessários.
- Usar construtores primários para DI (`public class(...)`).
- Nomes de arquivos e pastas em PascalCase.
- C# 12+ com nullable habilitado.
