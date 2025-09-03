# DESAFIO1


Solução em .NET **9.0** com arquitetura em camadas:
- **src/Common** — utilitários/contratos compartilhados
- **src/Domain** — entidades, agregados e regras de domínio
- **src/Application** — casos de uso (CQRS/Handlers/Services)
- **src/Infrastructure** — EF Core, repositórios, integrações
- **src/API** — camada de apresentação (Controllers/Minimal APIs)


## Requisitos
- .NET SDK 9.0+
- Docker Desktop (opcional, para rodar Postgres + API com compose)


## Como rodar (sem Docker)
1. Configure a connection string (PostgreSQL) via **variável de ambiente**:
```powershell
$env:ConnectionStrings__Default="Host=localhost;Port=5432;Database=desafio1;Username=postgres;Password=postgres"
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update --project src/Infrastructure --startup-project src/API
dotnet run --project src/API
