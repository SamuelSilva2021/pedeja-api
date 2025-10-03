# Docker Setup - PedejaApp

Este documento explica como executar o projeto PedejaApp usando Docker.

## Pré-requisitos

- Docker Desktop instalado
- Docker Compose (incluído no Docker Desktop)

## Arquivos Docker Criados

- **Dockerfile**: Configuração para build da aplicação ASP.NET Core 9.0
- **docker-compose.yml**: Orquestração da aplicação + PostgreSQL
- **.dockerignore**: Otimização do contexto de build

## Como Executar

### Opção 1: Usando Docker Compose (Recomendado)

```bash
# Executar aplicação + banco de dados
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar serviços
docker-compose down
```

### Opção 2: Apenas a Aplicação (Docker)

```bash
# Build da imagem
docker build -t pedeja-api .

# Executar container (necessário PostgreSQL externo)
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="server=host.docker.internal;Database=pedeja;port=5432;uid=postgres;pwd=admin;Encoding=UTF8;" \
  pedeja-api
```

## Acessos

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **PostgreSQL**: localhost:5432
  - Usuário: postgres
  - Senha: admin
  - Database: pedeja

## Volumes Persistentes

- **postgres_data**: Dados do PostgreSQL
- **uploads_data**: Arquivos de upload da aplicação

## Variáveis de Ambiente Importantes

```env
# Connection String
ConnectionStrings__DefaultConnection=server=postgres;Database=pedeja;port=5432;uid=postgres;pwd=admin;Encoding=UTF8;

# JWT Configuration
Jwt__Key=your_super_secret_jwt_key_here_change_in_production
Jwt__Issuer=PedejaApp
Jwt__Audience=PedejaApp

# File Storage
FileStorage__BaseUrl=http://localhost:8080/uploads
```

## Comandos Úteis

```bash
# Rebuild e restart
docker-compose up -d --build

# Ver status dos containers
docker-compose ps

# Executar migrations (se necessário)
docker-compose exec pedeja-api dotnet ef database update

# Acessar container da aplicação
docker-compose exec pedeja-api bash

# Acessar PostgreSQL
docker-compose exec postgres psql -U postgres -d pedeja

# Limpar volumes (CUIDADO: apaga dados)
docker-compose down -v
```

## Troubleshooting

### Problema: Porta já em uso
```bash
# Verificar processos usando a porta
netstat -ano | findstr :8080
netstat -ano | findstr :5432

# Alterar portas no docker-compose.yml se necessário
```

### Problema: Permissões de arquivo
```bash
# No Windows, certificar que Docker Desktop tem acesso ao drive
# Verificar configurações em Docker Desktop > Settings > Resources > File Sharing
```

### Problema: Build falha
```bash
# Limpar cache do Docker
docker system prune -a

# Rebuild sem cache
docker-compose build --no-cache
```

## Segurança

⚠️ **IMPORTANTE**: 
- Altere a chave JWT em produção
- Use senhas seguras para o PostgreSQL
- Configure HTTPS em produção
- Não exponha portas desnecessárias em produção