# EcoCheck API

API REST do **EcoCheck**, um questionário educativo e anônimo sobre hábitos cotidianos ligados à água, energia, resíduos, consumo e mobilidade. Projeto extensionista do curso de Análise e Desenvolvimento de Sistemas.

> O EcoCheck é uma ferramenta educativa para reflexão sobre hábitos. A pontuação **não** mede a pegada ecológica real nem constitui avaliação científica.

## Tecnologias

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 + Npgsql
- PostgreSQL 17

## Estrutura

```
ecocheck-api/
├── src/EcoCheck.Api/
│   ├── Controllers/            # endpoints HTTP
│   ├── Dtos/                   # contratos de entrada e saída
│   ├── Services/               # pontuação, classificação, validação e persistência
│   ├── Entities/               # modelo de domínio
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/     # mapeamento EF Core por entidade
│   │   ├── Seed/               # perguntas e alternativas do questionário
│   │   └── Migrations/
│   ├── Infrastructure/         # CORS, rate limiting, erros, proxy, DATABASE_URL
│   └── Program.cs
├── tests/EcoCheck.Api.Tests/   # testes xUnit
├── Dockerfile                  # imagem de produção
├── railway.json                # configuração de build/deploy do Railway
├── docker-compose.yml          # PostgreSQL local
├── global.json                 # fixa o SDK .NET 9
└── nuget.config                # usa apenas o nuget.org
```

## Executando localmente

### Pré-requisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL: via [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recomendado) **ou** uma instalação local do PostgreSQL

### 1. Subir o banco

Com Docker:

```bash
docker compose up -d
```

Sem Docker: crie no seu PostgreSQL um usuário `ecocheck` com senha `ecocheck_dev` e um banco `ecocheck` pertencente a ele (ou ajuste a connection string em `appsettings.Development.json`).

### 2. Restaurar ferramentas e executar

```bash
dotnet tool restore
dotnet run --project src/EcoCheck.Api
```

Em desenvolvimento, as migrations são aplicadas automaticamente na inicialização (`Database:ApplyMigrationsOnStartup = true`). A API sobe em `http://localhost:5080` e o documento OpenAPI fica em `/openapi/v1.json`.

### Migrations (manual)

```bash
# criar uma nova migration após alterar entidades
dotnet ef migrations add NomeDaMigration --project src/EcoCheck.Api --output-dir Data/Migrations

# aplicar no banco configurado
dotnet ef database update --project src/EcoCheck.Api
```

### Testes

```bash
dotnet test
```

## Endpoints

### `GET /api/questionnaire`

Retorna a versão do questionário e as perguntas ativas com suas alternativas. A pontuação das alternativas não é exposta; `isNotApplicable` indica a opção "Não se aplica".

```json
{
  "version": 1,
  "questions": [
    {
      "id": 1, "category": "water", "order": 1,
      "text": "Quanto tempo, em média, dura o seu banho?",
      "options": [{ "id": 11, "text": "Até 5 minutos", "isNotApplicable": false }]
    }
  ]
}
```

### `POST /api/responses`

Recebe as respostas anônimas, valida, calcula o resultado no servidor e salva. Corpo limitado a 16 KB.

```json
{
  "questionnaireVersion": 1,
  "answers": [{ "questionId": 1, "optionId": 12 }],
  "countryCode": "BR",
  "stateCode": "SP"
}
```

`countryCode` (ISO 3166-1 alfa-2) e `stateCode` (sigla da UF) são **opcionais** e não afetam a pontuação. O estado só é aceito quando o país é `BR`.

Resposta `200 OK`:

```json
{
  "totalScore": 43, "maxScore": 68, "percentage": 63.24,
  "classification": "good_habits",
  "categories": [{ "category": "water", "score": 14, "maxScore": 16, "percentage": 87.5 }],
  "strengthQuestionIds": [1, 4, 7],
  "improvementQuestionIds": [6, 13, 15]
}
```

Resposta `400` no formato `ValidationProblemDetails` quando falta alguma pergunta, há respostas duplicadas, alternativas que não pertencem à pergunta ou versão desatualizada do questionário.

### Regras de pontuação

- Cada alternativa vale de 0 a 4 pontos; "Não se aplica" é excluída da pontuação **e** do máximo possível.
- Percentual = pontos obtidos ÷ pontos máximos aplicáveis × 100 (duas casas decimais).
- Classificação: `starting` [0–20], `first_steps` (20–40], `on_track` (40–60], `good_habits` (60–80], `inspiring` (80–100].
- `strengthQuestionIds`: perguntas com pontuação máxima. `improvementQuestionIds`: perguntas com até 50% da pontuação, da menor para a maior.

### `GET /api/statistics`

Retorna **apenas dados agregados** das participações (cache em memória de 60 s por filtro, invalidado a cada nova participação).

**Filtro opcional por região:**

| Requisição | Considera |
|---|---|
| `GET /api/statistics` | Todas as participações |
| `GET /api/statistics?country=BR` | Participações de um país (ISO 3166-1 alfa-2) |
| `GET /api/statistics?country=BR&state=SP` | Participações de uma UF (somente com `country=BR`) |

Se a região filtrada tiver **menos de 5 participantes**, a resposta vem com `summaryAvailable: false` e nenhum agregado (médias, distribuições e até o total, que vem como `0`), para não expor resultados individuais. As regiões que podem ser filtradas com dados são as listadas em `regions` da consulta sem filtro. Códigos inválidos retornam `400`.

```json
{
  "filter": { "countryCode": null, "stateCode": null },
  "summaryAvailable": true,
  "totalParticipants": 42,
  "averagePercentage": 61.8,
  "categories": [{ "category": "water", "averagePercentage": 58.3 }],
  "classifications": [{ "classification": "good_habits", "count": 17, "percentage": 40.48 }],
  "detailsAvailable": true,
  "minimumParticipantsForDetails": 5,
  "questions": [{
    "questionId": 11, "category": "waste", "text": "Você separa os materiais recicláveis...",
    "totalAnswers": 42, "averagePercentage": 62.5,
    "options": [{ "optionId": 111, "text": "Sempre", "isNotApplicable": false, "count": 17, "percentage": 40.48 }]
  }],
  "topHabits": [{ "questionId": 6, "category": "energy", "text": "...", "averagePercentage": 88.1 }],
  "improvementOpportunities": [{ "questionId": 4, "category": "water", "text": "...", "averagePercentage": 21.4 }],
  "regions": {
    "participantsWithRegion": 30,
    "countries": [{ "code": "BR", "participants": 28, "averagePercentage": 60.9 }],
    "brazilStates": [{ "code": "SP", "participants": 12, "averagePercentage": 63.2 }]
  },
  "generatedAt": "2026-09-30T18:00:00+00:00"
}
```

**Privacidade:** a distribuição por pergunta e os destaques (`questions`, `topHabits`, `improvementOpportunities`) só são retornados a partir de **5 participantes** (`detailsAvailable`). Da mesma forma, países e estados só aparecem em `regions` quando têm pelo menos 5 participantes. Com poucas respostas, esses dados poderiam revelar escolhas individuais.

### `GET /health`

Verifica a aplicação e a conexão com o banco. Usado pelo health check do Railway.

## Segurança

| Medida | Implementação |
|---|---|
| Validação | DataAnnotations + validação das respostas contra as perguntas ativas; erros em `ProblemDetails` em português |
| Payload | Corpo do `POST` limitado a 16 KB; JSON malformado retorna 400 sem expor detalhes internos |
| Pontuação | Calculada somente no servidor; o cliente envia apenas ids |
| CORS | Somente as origens em `Cors__AllowedOrigins`, métodos `GET`/`POST` e header `Content-Type` |
| Rate limiting | Por IP, só em memória: `POST /api/responses` 5 a cada 10 min; leituras 60/min. Excesso → `429` com `Retry-After` |
| Erros | Handler global (`IExceptionHandler`): nenhuma stack trace ou mensagem interna chega ao cliente |
| HTTPS | Em produção: redirecionamento para HTTPS e HSTS (1 ano), respeitando o proxy do Railway |
| Headers | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`; header `Server` removido |
| Segredos | Nenhum no código; conexão com o banco apenas por variável de ambiente |
| Container | Imagem oficial `aspnet:9.0` executando com usuário sem privilégios |

**Privacidade:** o IP é usado apenas em memória para o rate limiting e nunca é gravado no banco ou em logs. Não há autenticação de participantes. Uma área administrativa futura pode ser adicionada em `Controllers/Admin/` com autenticação própria, sem mudanças no modelo de dados.

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `DATABASE_URL` | sim* | URI do PostgreSQL (`postgresql://user:pass@host:port/db`), fornecida pelo Railway |
| `ConnectionStrings__Default` | sim* | Alternativa no formato Npgsql (`Host=...;Database=...;Username=...;Password=...`). Tem prioridade sobre `DATABASE_URL` |
| `Cors__AllowedOrigins` | sim (produção) | Origens do front-end separadas por vírgula, ex.: `https://ecocheck.vercel.app` |
| `Database__ApplyMigrationsOnStartup` | não | `true` para aplicar migrations ao iniciar (padrão: `false` em produção, `true` em desenvolvimento) |
| `ReverseProxy__Enabled` | não | Processa `X-Forwarded-For`/`X-Forwarded-Proto` (padrão: `true` em produção, `false` em desenvolvimento) |
| `RateLimiting__SubmitPermitLimit` | não | Envios por IP por janela (padrão `5`) |
| `RateLimiting__SubmitWindowMinutes` | não | Janela dos envios em minutos (padrão `10`) |
| `RateLimiting__ReadPermitLimit` | não | Leituras por IP por janela (padrão `60`) |
| `RateLimiting__ReadWindowSeconds` | não | Janela das leituras em segundos (padrão `60`) |
| `PORT` | não | Porta HTTP; definida automaticamente pelo Railway |

\* Uma das duas.

Nenhum segredo de produção fica no repositório. A connection string de `appsettings.Development.json` vale apenas para o banco local do `docker-compose.yml`.

## Deploy no Railway

O repositório já contém `Dockerfile` e `railway.json` (build via Dockerfile, health check em `/health`, reinício em caso de falha).

1. Suba o repositório `ecocheck-api` para o GitHub.
2. Em [railway.com](https://railway.com), crie um projeto: **New Project → Deploy from GitHub repo → ecocheck-api**.
3. No mesmo projeto, adicione o banco: **+ New → Database → PostgreSQL**.
4. No serviço da API, aba **Variables**, adicione:
   ```
   DATABASE_URL=${{Postgres.DATABASE_URL}}
   Database__ApplyMigrationsOnStartup=true
   Cors__AllowedOrigins=https://SEU-PROJETO.vercel.app
   ```
   `${{Postgres.DATABASE_URL}}` é uma referência do Railway ao banco criado no passo 3 (use o nome exato do serviço do banco, se for diferente) e usa a rede privada do projeto.
5. Em **Settings → Networking**, clique em **Generate Domain** para obter a URL pública (`https://...up.railway.app`).
6. Aguarde o deploy e acesse `https://SUA-API.up.railway.app/health`; a resposta deve ser `Healthy`.
7. Depois de publicar o front-end na Vercel, atualize `Cors__AllowedOrigins` com o domínio final (o Railway faz um novo deploy automaticamente).

Cada `git push` na branch principal gera um novo deploy. As migrations pendentes são aplicadas na inicialização enquanto `Database__ApplyMigrationsOnStartup=true`.

> O rate limiting fica em memória, adequado para uma única instância da API. Com várias réplicas, cada uma teria seu próprio contador.

## Modelo de dados

| Tabela | Conteúdo |
|---|---|
| `questions` | Perguntas (categoria, texto, ordem, ativa) |
| `question_options` | Alternativas com pontuação 0–4; `NULL` = "Não se aplica" |
| `survey_responses` | Participação anônima: pontuação total, máximo, percentual, classificação e, se informados, país e UF |
| `survey_answers` | Alternativa escolhida por pergunta (com cópia da pontuação) |
| `survey_category_scores` | Pontuação por categoria de cada participação |

Nenhuma tabela armazena nome, e-mail, IP, cidade, localização precisa ou qualquer dado pessoal. A região, quando informada, se limita a país e estado.
