# Desafio – API .NET (Clientes, Produtos e Pedidos)

API REST em .NET 9, DDD (Domain, Application, Infrastructure, API), PostgreSQL, EF Core, JWT + Basic, Swagger e Serilog em arquivo.

## 🔧 Requisitos
- .NET SDK 9.0+
- Docker e Docker Compose
- PowerShell (Windows) ou shell compatível

---

## ▶️ Como rodar com Docker (recomendado)

1. **Ajuste o segredo JWT no `docker-compose.yml`** (`Jwt__Secret`) – use um valor forte (≥ 64 chars).
2. **Suba tudo**:
   ```powershell
   docker compose up -d --build
