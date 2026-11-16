# ViSah API

API REST para o catálogo musical ViSah. O projeto usa ASP.NET Core, EF Core com SQL Server, JWT, Scalar e Cloudinary.

## Configuração local

O `appsettings.json` contém a estrutura e valores padrão. Coloque a chave JWT, a conexão real do SQL Server e as credenciais Cloudinary nos User Secrets; não salve segredos no repositório.

```powershell
dotnet restore ViSah.sln
dotnet user-secrets init --project ViSah/ViSah.csproj

# Substitua pelo nome da instância SQL Server exibida no SSMS.
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=SEU_SERVIDOR_SQL;Database=ViSahDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project ViSah/ViSah.csproj

# Gera uma chave aleatória adequada para assinar tokens JWT.
$jwtKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
dotnet user-secrets set "Jwt:Key" $jwtKey --project ViSah/ViSah.csproj
Remove-Variable jwtKey

# Necessário para enviar e substituir capas de músicas e álbuns.
dotnet user-secrets set "Cloudinary:CloudName" "seu-cloud-name" --project ViSah/ViSah.csproj
dotnet user-secrets set "Cloudinary:ApiKey" "sua-api-key" --project ViSah/ViSah.csproj
dotnet user-secrets set "Cloudinary:ApiSecret" "seu-api-secret" --project ViSah/ViSah.csproj

# Aplica todas as migrations pendentes e cria o banco se ele ainda não existir.
dotnet ef database update --project ViSah/ViSah.csproj --startup-project ViSah/ViSah.csproj

# Inicia a API no perfil Development.
dotnet run --project ViSah/ViSah.csproj
```

Se `dotnet ef` não for reconhecido, instale a ferramenta uma vez com `dotnet tool install --global dotnet-ef --version 10.0.14`.

Scalar abre em `https://localhost:7051/scalar` ou `http://localhost:5291/scalar`. Se sua instância do SQL Server usar autenticação SQL, substitua a connection string pelo formato com `User Id` e `Password`, e grave a senha apenas no User Secrets.

O endpoint de capas aceita `multipart/form-data` com o campo `image`. Apenas administradores autenticados podem enviar arquivos. São aceitos JPG, PNG, WEBP e GIF até 10 MB; a API valida o tipo declarado e a assinatura do arquivo. A URL segura e o `public_id` são guardados no SQL Server. Ao substituir uma capa, a imagem anterior é removida do Cloudinary.

- `POST /api/tracks/{id}/cover`
- `POST /api/albums/{id}/cover`

## Reprodução

`GET /api/tracks/{id}/audio` redireciona o player para a URL HTTPS cadastrada para a faixa. O endpoint `POST /api/tracks/{id}/play` registra a reprodução autenticada.

## Banco de dados

```powershell
dotnet ef database update --project ViSah/ViSah.csproj --startup-project ViSah/ViSah.csproj
```

Em desenvolvimento, a documentação OpenAPI e o Scalar estão disponíveis ao iniciar a API. O OpenAPI declara o esquema `BearerAuth` nos endpoints protegidos, e o Scalar mostra a autorização JWT. Faça login em `POST /api/auth/login`, copie o valor de `token` para o campo de autorização do Scalar e cole somente o token, sem escrever `Bearer `.
