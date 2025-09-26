# Dockerfile para PedejaApp - ASP.NET Core 9.0
# Multi-stage build para otimizar tamanho da imagem final

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto primeiro para aproveitar cache do Docker
COPY ["PedejaApp.Web/PedejaApp.Web.csproj", "PedejaApp.Web/"]
COPY ["PedejaApp.Application/PedejaApp.Application.csproj", "PedejaApp.Application/"]
COPY ["PedejaApp.Domain/PedejaApp.Domain.csproj", "PedejaApp.Domain/"]
COPY ["PedejaApp.Infrastructure/PedejaApp.Infrastructure.csproj", "PedejaApp.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "PedejaApp.Web/PedejaApp.Web.csproj"

# Copiar todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/PedejaApp.Web"
RUN dotnet build "PedejaApp.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "PedejaApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Criar usuário não-root para segurança
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Criar diretório para uploads
RUN mkdir -p wwwroot/uploads

# Configurar variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expor porta
EXPOSE 8080

# Comando de entrada
ENTRYPOINT ["dotnet", "PedejaApp.Web.dll"]