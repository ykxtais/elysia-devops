# ElysiaAPI 📱🏍️

API RESTful desenvolvida em .NET 8 com Entity Framework Core e Oracle, parte do projeto **Elysia: Inteligência para Gestão Inteligente de Pátios** da empresa Mottu. Esta API permite o gerenciamento de **motos** e **vagas de estacionamento**, com foco em uma solução inteligente para controle de pátios.

## 👥 Integrantes
Iris Tavares Alves - 557728 - 2TDSPM

Taís Tavares Alves - 557553 - 2TDSPM

## ⚙️ Tecnologias Utilizadas

```text
- ASP.NET Core 8
- Entity Framework Core
- Oracle Database
- Swagger (OpenAPI)
- Clean Architecture (camadas Domain, Infrastructure, Application)
```

1. Clone o repositório
```text
https://github.com/Irissuu/cp2sharp.git
cd ElysiaAPI
```

2. Configure a string de conexão
```text
"ConnectionStrings": {
  "OracleDB": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl;"
}
```

3. Instale os pacotes
```text
dotnet restore
```

4. Gere o banco de dados com EF Core
```text
dotnet ef migrations add Inicial
dotnet ef database update
```

5. Execute o projeto
```text
dotnet run
```
