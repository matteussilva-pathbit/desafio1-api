
# Desafio: API de Clientes, Produtos e Pedidos (.NET)

API REST em **.NET 9 / C#** com DDD (Domain, Application, Infrastructure, Api), autenticação **JWT** e **Basic**, **EF Core** + **PostgreSQL**, **Swagger** e **logs** via Serilog.

---

## 🎯 Requisitos atendidos

- CRUDs essenciais (Clientes, Produtos, Pedidos) e regras de negócio
- Autenticação **/signup**, **/login** (JWT 1h)
- Autorização por **roles** (ADMIN, CLIENTE)
- **PostgreSQL** via EF Core
- **Swagger** habilitado
- **Logs** em `./logs/application.log`
- **Testes unitários** (xUnit + Moq + FluentAssertions) com **cobertura ≥ 70%**  
  (resultado de referência: **Line 83.7%**, **Branch 76.7%**)

---

## 🚀 Como rodar localmente

### Pré-requisitos
- .NET SDK 9
- PostgreSQL rodando e acessível
- (Opcional) ReportGenerator:
  ```bash
  dotnet tool install -g dotnet-reportgenerator-globaltool

*********************************************************************************************************************************
Variáveis de ambiente (exemplo)

Configure no appsettings.json ou em variáveis de ambiente:

{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=desafio;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "dev-secret-change-me-please"
  }
}
*********************************************************************************************************************************
Migrar banco (se necessário)
dotnet ef database update --project src/Infrastructure --startup-project src/Api

***********************************************************************************************************************************

Rodar API
dotnet build
dotnet run --project src/Api/Api.csproj

***********************************************************************************************************************************

Swagger:

https://localhost:xxxx/swagger

************************************************************************************************************************************
Docker:

docker build -t desafio-api ./src/Api
docker run -p 8080:8080 -e ConnectionStrings__Default="Host=host.docker.internal;Port=5432;Database=desafio;Username=postgres;Password=postgres" -e Jwt__Secret="dev-secret-change-me-please" desafio-api
docker compose up --build
**************************************************************************************************************************************************

🔐 Autenticação
1) Cadastro (/signup) — público

Envia nome, email e senha

Cria registros em User (senha SHA256) e Customer

2) Login (/login) — público

Envia usuario (email) e senha

Retorna JWT válido por 1 hora com (CustomerId, Nome, Email)

3) Autorização por role

ADMIN pode cadastrar/alterar produtos

CLIENTE pode criar pedidos

No Swagger, clique em Authorize:

JWT: selecione Bearer, cole somente o token (sem Bearer ).

Basic: selecione Basic (username:senha em Base64).

**************************************************************************************************************************************************

🧪 Testes & Cobertura (≥ 70%)
Rodar testes com cobertura (Coverlet)
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

Gerar relatório (ReportGenerator)
reportgenerator -reports:tests/UnitTests/TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:"HtmlInline;TextSummary"


Abrir:

start ./coveragereport/index.html   # Windows
# ou
open ./coveragereport/index.html    # macOS

Arquivo coverlet.runsettings

Já incluso no repositório. Ele exclui arquivos que não agregam valor à métrica (DTOs/Models, Program, Migrations etc.) para refletir melhor a cobertura de regras de negócio.

******************************************************************************************************************************************************
📁 Estrutura (DDD)
/src
  /Api
  /Application
  /Common
  /Domain
  /Infrastructure
/tests
  /UnitTests
/scripts/db
  init.sql

  ****************************************************************************************************************************************************
  🪵 Logs

Serilog escreve em ./logs/application.log (rotaciona por dia)

Garanta que logs/ está no .gitignore

*****************************************************************************************************************************************************

🧰 Endpoints principais

POST /signup

POST /login

POST /products (ADMIN)

PUT /products/{id}/price (ADMIN)

PUT /products/{id}/inventory (ADMIN)

POST /orders (CLIENTE; valida CEP via viacep.com.br; usa customerId do JWT; debita estoque com transação)

********************************************************************************************************************************************************

