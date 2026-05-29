---
name: development-rules
description: >
  Regras de desenvolvimento do projeto Tibar. Use quando for criar branch,
  commitar, fazer PR, configurar ambiente ou escrever código C#/Angular.
  Trigger: branch, commit, PR, code style, git flow, conventional commit, setup,
  teste, docker compose.
---

# Regras de Desenvolvimento — Tibar

## Git Flow

- **Nunca** commitar diretamente na `main`.
- **Toda e qualquer alteração no código** (features, fixes, refactors, chores, docs, testes) **deve começar com uma branch limpa a partir da `main`**.
- **Nunca reaproveitar uma branch existente** — cada alteração tem sua própria branch.
- Prefixos obrigatórios:
  - `fix/` para correções
  - `feat/` para novas funcionalidades
  - `chore/` para tarefas de manutenção
- Abrir Pull Request e fazer merge para `main` após revisão.

```bash
git checkout main
git pull origin main
git checkout -b feat/nome-da-feature
git commit -m "feat: descrição"
git push origin feat/nome-da-feature
```

## Conventional Commits

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>: <descrição em inglês>

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

## Testes

```bash
dotnet test
```

Sempre execute os testes antes de abrir um PR.

## Code Style

- Sempre seguir os padrões existentes no código.
- **Sem comentários desnecessários** no código.
- Usar construtores primários para DI (`public class(...)`).
- Nomes de arquivos e pastas em PascalCase.
- C# 12+ com nullable habilitado.
- Result pattern: toda action verifica `result.IsValid` antes de retornar.
- Mensagens de erro em português (PT-BR).
