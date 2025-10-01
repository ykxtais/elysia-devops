# ⟢ Elysia API – Deploy

Processo completo de deploy da API Elysia em Azure App Service, utilizando .NET 8 e PostgreSQL. </br>
Inclui os scripts de criação da infraestrutura, publicação via publish.zip, configuração do ambiente e validação da aplicação.

## ⚙️ Pré-requisitos
- Conta Microsoft Azure.
- Azure CLI configurado (ou Cloud Shell no portal).
- .NET 8 SDK instalado localmente.
- Banco de dados PostgreSQL no Azure (será criado via script).

## 1. Clonar o Repositório
```
git clone https://github.com/ykxtais/elysia-devops.git
cd elysia-devops
```

## 2. Compile e gere os artefatos de publicação
```
dotnet publish -c Release -o publish
```
zip o arquivo gerado, carrege/upload o arquivo publish.zip e mova para pasta do projeto.

## 3. Provisionando a Infraestrutura no Azure
- Scripts de Provisionamento
  - `postgresql.sh`
  - `appServiceElysia.sh`
  - `insightsElysia.sh`

postgresql.sh
```
chmod +x postgresql.sh
./postgresql.sh
```
No Cloud Shell (Bash), execute o script incluso no repositório:

```bash
#!/usr/bin/env bash
set -euo pipefail

# variáveis 
RG="rg-elysia"
LOC="brazilsouth"

PLAN="plan-elysia"
APP="elysia-api"
RUNTIME="DOTNETCORE|8.0"

APP_INSIGHTS_NAME="ai-elysia"

PG_SERVER="elysiapg"
PG_DB="elysia_devops"
PG_ADMIN="pgadmin"
PG_ADMIN_PWD='Elysia@admin25'
PG_APP_USER="elysia_app"
PG_APP_PWD='Elysia@app25'
PG_HOST="${PG_SERVER}.postgres.database.azure.com"

# criando resource Group 
az group create --name "$RG" --location "$LOC" >/dev/null

# criando postgres flexible-server
az postgres flexible-server create \
  --resource-group "$RG" \
  --name "$PG_SERVER" \
  --location "$LOC" \
  --admin-user "$PG_ADMIN" \
  --admin-password "$PG_ADMIN_PWD" \
  --public-access 0.0.0.0 \
  --version 16 \
  --tier Burstable \
  --sku-name Standard_B1ms \
  --storage-size 32

# criando database lógico
az postgres flexible-server db create \
  --resource-group "$RG" \
  --server-name "$PG_SERVER" \
  --database-name "$PG_DB"

# criando usuário da aplicação e concedendo permissões
cat > create_app_user.sql <<SQL
DO \$\$ BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${PG_APP_USER}') THEN
      CREATE ROLE ${PG_APP_USER} LOGIN PASSWORD '${PG_APP_PWD}';
   END IF;
END \$\$;

GRANT CONNECT ON DATABASE ${PG_DB} TO ${PG_APP_USER};
GRANT USAGE ON SCHEMA public TO ${PG_APP_USER};
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO ${PG_APP_USER};
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO ${PG_APP_USER};
SQL

# criando DDL + inserts 
cat > script_bd.sql <<'SQL'
CREATE TABLE IF NOT EXISTS "MotoCsharp" (
    "Id"      INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Placa"   VARCHAR(8)  NOT NULL,
    "Marca"   VARCHAR(50) NOT NULL,
    "Modelo"  VARCHAR(50) NOT NULL,
    "Ano"     INTEGER     NOT NULL
);

CREATE TABLE IF NOT EXISTS "UsuarioCsharp" (
    "Id"     INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nome"   VARCHAR(80)  NOT NULL,
    "Email"  VARCHAR(254) NOT NULL,
    "Senha"  VARCHAR(120) NOT NULL,
    "Cpf"    VARCHAR(11)  NOT NULL
);

CREATE TABLE IF NOT EXISTS "VagaCsharp" (
    "Id"     INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Status" VARCHAR(20)  NULL,
    "Numero" INTEGER      NOT NULL,
    "Patio"  VARCHAR(50)  NOT NULL
);

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'uk_usuariocsharp_email') THEN
        CREATE UNIQUE INDEX uk_usuariocsharp_email ON "UsuarioCsharp"("Email");
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'uk_usuariocsharp_cpf') THEN
        CREATE UNIQUE INDEX uk_usuariocsharp_cpf   ON "UsuarioCsharp"("Cpf");
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'uk_vaga_patio_numero') THEN
        CREATE UNIQUE INDEX uk_vaga_patio_numero   ON "VagaCsharp"("Patio","Numero");
    END IF;
END $$;

INSERT INTO "VagaCsharp"("Status","Numero","Patio") VALUES ('Livre',  1, 'Mottu Vila Mariana');
INSERT INTO "VagaCsharp"("Status","Numero","Patio") VALUES ('Ocupada',  2, 'Mottu Vila Mariana');
SQL

# aplicando scripts no servidor 
PGPASSWORD="$PG_ADMIN_PWD" psql "host=$PG_HOST port=5432 dbname=$PG_DB user=$PG_ADMIN sslmode=require" -f create_app_user.sql
PGPASSWORD="$PG_ADMIN_PWD" psql "host=$PG_HOST port=5432 dbname=$PG_DB user=$PG_ADMIN sslmode=require" -f script_bd.sql

# arquivo .env.elysia com a connection string
PG_CONN="Host=$PG_HOST;Port=5432;Database=$PG_DB;Username=$PG_APP_USER;Password=$PG_APP_PWD;Ssl Mode=Require;"

echo "PG_CONN=\"$PG_CONN\"" > .env.elysia
echo "connection String salva em ./.env.elysia"
```
- Observação: o CLI não instala psql; aqui assumimos que você roda no Cloud Shell com psql disponível ou em ambiente onde o psql está instalado.

Esse script cria/configura:
- Resource Group
- Servidor PostgreSQL
- Banco de dados
- Regras de firewall para acesso
- script `script_bd.sql`
- Usuário de aplicação
- Admin do servidor

### 3.1. Provisionando a Infraestrutura no Azure

appServiceElysia.sh
```
chmod +x appServiceElysia.sh
./appServiceElysia.sh
```
No Cloud Shell (Bash), execute o script incluso no repositório:
```bash
#!/usr/bin/env bash
set -euo pipefail

# variáveis
RG="rg-elysia"
LOC="brazilsouth"

PLAN="plan-elysia"
APP="elysia-api"
RUNTIME="DOTNETCORE|8.0"

APP_INSIGHTS_NAME="ai-elysia"

PG_SERVER="elysiapg"
PG_DB="elysia_devops"
PG_ADMIN="pgadmin"
PG_ADMIN_PWD='Elysia@admin25'
PG_APP_USER="elysia_app"
PG_APP_PWD='Elysia@app25'
PG_HOST="${PG_SERVER}.postgres.database.azure.com"

# criando app service plan
az appservice plan create \
  --name "$PLAN" \
  --resource-group "$RG" \
  --location "$LOC" \
  --sku B1 \
  --is-linux

# criando web app (.NET 8)
az webapp create \
  --resource-group "$RG" \
  --plan "$PLAN" \
  --name "$APP" \
  --runtime "$RUNTIME"

# lendo/montando connection string do arquivo .env.elysia 
if [ -f .env.elysia ]; then
  source .env.elysia
else
  PG_CONN="Host=$PG_HOST;Port=5432;Database=$PG_DB;Username=$PG_APP_USER;Password=$PG_APP_PWD;Ssl Mode=Require;"
fi

# configurando ConnectionStrings:PostgresDB no Web App
az webapp config connection-string set \
  --resource-group "$RG" \
  --name "$APP" \
  --settings PostgresDB="$PG_CONN" \
  --connection-string-type PostgreSQL

# faça upload do publish.zip e rode:
az webapp deploy \
  --resource-group "$RG" \
  --name "$APP" \
  --src-path ./publish.zip \
  --type zip
```
Esse script cria/configura:
- App Service Plan
- Web App
- Connection string
- Deploy do publish.zip

### 3.2. Provisionando a Infraestrutura no Azure

insightsElysia.sh
```
chmod +x insightsElysia.sh
./insightsElysia.sh
```
No Cloud Shell (Bash), execute o script incluso no repositório:
```bash
#!/usr/bin/env bash
set -euo pipefail

# variáveis comuns
RG="rg-elysia"
LOC="brazilsouth"

PLAN="plan-elysia"
APP="elysia-api"
RUNTIME="DOTNETCORE|8.0"

APP_INSIGHTS_NAME="ai-elysia"

PG_SERVER="elysiapg"
PG_DB="elysia_devops"
PG_ADMIN="pgadmin"
PG_ADMIN_PWD='Elysia@admin25'
PG_APP_USER="elysia_app"
PG_APP_PWD='Elysia@app25'
PG_HOST="${PG_SERVER}.postgres.database.azure.com"

# criando application insights
az monitor app-insights component create \
  --app "$APP_INSIGHTS_NAME" \
  --location "$LOC" \
  --resource-group "$RG" \
  --application-type web

# obtendo connectionString do insights
CONNECTION_STRING=$(az monitor app-insights component show \
  --app "$APP_INSIGHTS_NAME" \
  --resource-group "$RG" \
  --query connectionString -o tsv)

# configurando app settings no web app
az webapp config appsettings set \
  --name "$APP" \
  --resource-group "$RG" \
  --settings \
  APPLICATIONINSIGHTS_CONNECTION_STRING="$CONNECTION_STRING" \
  ApplicationInsightsAgent_EXTENSION_VERSION="~3" \
  XDT_MicrosoftApplicationInsights_Mode="Recommended" \
  XDT_MicrosoftApplicationInsights_PreemptSdk="1"

# conectando insights ao web app
az monitor app-insights component connect-webapp \
  --app "$APP_INSIGHTS_NAME" \
  --web-app "$APP" \
  --resource-group "$RG"

# reiniciando web app
az webapp restart --name "$APP" --resource-group "$RG"

# application Insights configurado
```
Esse script cria/configura:
- Application Insights
- Conecta o Web App ao recurso do Insights

## 4. Testando a API
⟢ Acesse o Swagger em: ``` https://elysia-api.azurewebsites.net/swagger ``` </br>
⟢ azurewebsites endpoints: ``` https://elysia-api.azurewebsites.net/api/vaga ``` </br> ``` https://elysia-api.azurewebsites.net/api/moto ``` </br> ``` https://elysia-api.azurewebsites.net/api/usuario ```

</br>

## 🖇 Rotas Disponíveis (via Swagger)

### MotoController

| Método | Rota                            | Descrição                          |
|--------|----------------------------------|-------------------------------------|
| GET    | `/api/moto`                     | Lista todas as motos                |
| GET    | `/api/moto/{id}`                | Busca uma moto por ID               |
| GET    | `/api/moto/search?placa=XXX`    | Busca motos por placa (parcial)     |
| POST   | `/api/moto`                     | Cadastra uma nova moto              |
| PUT    | `/api/moto/{id}`                | Atualiza uma moto existente         |
| DELETE | `/api/moto/{id}`                | Remove uma moto                     |

### VagaController

| Método | Rota                                | Descrição                           |
|--------|-------------------------------------|--------------------------------------|
| GET    | `/api/vaga`                         | Lista todas as vagas                 |
| GET    | `/api/vaga/{id}`                    | Busca uma vaga por ID                |
| GET    | `/api/vaga/patio?patio=XYZ`         | Lista vagas por pátio                |
| POST   | `/api/vaga`                         | Cadastra uma nova vaga               |
| PUT    | `/api/vaga/{id}`                    | Atualiza uma vaga existente          |
| DELETE | `/api/vaga/{id}`                    | Remove uma vaga                      |

### UsuarioController

| Método | Rota                                | Descrição                           |
|--------|-------------------------------------|--------------------------------------|
| GET    | `/api/usuario/{id}`                    | Busca um usuario por ID                |
| POST   | `/api/usuario`                         | Cadastra um novo usuario             |
| PUT    | `/api/usuario/{id}`                    | Atualiza um usuarioexistente          |
| DELETE | `/api/usuario/{id}`                    | Remove um usuario                    |

</br>

## 💾 Inserts (Swagger)

### POST Moto — `/api/moto`

```json
{
  "placa": "ABC2D34",
  "marca": "Yamaha",
  "modelo": "R7",
  "ano": 2024
}
```

### POST Usuario — `/api/usuario`
```json
{
  "nome": "Marina",
  "email": "mari@email.com",
  "senha": "Icarus39",
  "cpf": "98765432109"
}
```

### POST Vaga — `/api/vaga`
```json
{
  "status": "Livre",
  "numero": 15,
  "patio": "Externo"
}
```

</br>

## ❗️ Troubleshooting

- Erro 500 no swagger: Verifique `ConnectionStrings__PostgresDB` no App Service
- `Either './publish' is not a valid local file path or you do not have permissions to access it`: Verifique se o arquivo está no local correto, caso não esteja, mova-o para dentro da pasta do projeto, caso não seja isso, verifique compactar a pasta antes do deploy:
    - Linux/macOS → `zip -r publish.zip ./publish`
    - Windows (PowerShell) → `Compress-Archive -Path publish\* -DestinationPath publish.zip -Force`
        - Em seguida rode `./appServiceElysia.sh`

</br>

# ⟢ Integrantes

➤ Iris Tavares Alves — 557728 </br>
➤ Taís Tavares Alves — 557553 
