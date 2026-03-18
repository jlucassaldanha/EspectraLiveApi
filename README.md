# SpectraLive API 🚀

Uma API robusta construída em **C# .NET** (usando Minimal APIs) aplicando conceitos avançados de mercado, como *SOLID, *Clean Architecture*, *Result Pattern* para tratamento de erros, e uma separação rigorosa de contratos de comunicação (DTOs). 

Projetada para gerenciar visualizaçõa de usuários do chat da Twitch através de integrações com a Twitch.

O projeto está em **fase de desenvolvimento**. Foi desenvolvido inicialmente em **Python** e a presente refatoração do projeto esta servido para estudar e dominar o ecossistema .NET, além de aprofundar mais os conhecimentos de **SOLID** e **Clean Architecture**.

## 🛠️ Tecnologias Utilizadas

*   **Linguagem:** C# .NET 10 (Minimal APIs)
*   **Banco de Dados (Desenvolvimento):** SQLite (via Entity Framework Core) 
*   **Integrações:** Twitch API (Helix) & Twitch OAuth2
*   **Padrões de Projeto:** Result Pattern, Layered Architecture, Clean Architecture, SOLID, Injeção de Dependência.

## 🏗️ Arquitetura do Projeto

O projeto foge do padrão monolítico e adota uma separação clara de responsabilidades:

*   📁 **`Entities/`**: Representam as tabelas do banco de dados (ex: `User`). O núcleo de dados da aplicação.
*   📁 **`Common/`**: Contém lógicas e modelos compartilhados por toda a aplicação, incluindo o nosso padrão de resposta `Result<T>` e modelos de estado interno (ex: `UserData`).
*   📁 **`DTOs/`**: *Data Transfer Objects*. São os contratos de comunicação estritos com o mundo externo (o que entra e o que sai da API pro Frontend).
*   📁 **`Endpoints/`**: Nossos "Controllers" usando Minimal APIs. Recebem a requisição HTTP, acionam os serviços e mapeiam a resposta final.
*   📁 **`Services/`**: Onde a regra de negócio da aplicação vive de fato.
*   📁 **`Integrations/`**: Camada isolada responsável exclusivamente por fazer chamadas HTTP para serviços externos (ex: `TwitchApiClient`). Protege a aplicação das instabilidades de APIs de terceiros.
*   📁 **`Repositories/`**: Gerencia a persistência e comunicação direta com o banco de dados.

## ✨ Funcionalidades Atuais

Até o momento, a API é capaz de:

*   **Fluxo de Autenticação com a Twitch:**
    *   Endpoint de Callback para processar o código de autorização da Twitch.
    *   Troca segura do código por *Access Tokens* e *Refresh Tokens*.
*   **Gerenciamento de Sessão Interna:**
    *   Criação de tokens de sessão temporários salvos em cache/cookies de forma segura para o Frontend.
*   **Perfil de Usuário:**
    *   Busca de dados do perfil do usuário no banco de dados ou diretamente na API da Twitch de forma autenticada, convertendo as respostas brutas da Twitch em contratos limpos para o frontend.
*   **Tratamento de Erros Padronizado:**
    *   Uso do `Result Pattern` para capturar falhas da Twitch ou de regras de negócio internas sem o uso excessivo (e custoso) de exceções.
    *   Conversão automática de falhas internas para *HTTP Status Codes* semânticos (400, 401, 404).


## 📜 Licença
Este projeto está licenciado sob a **MIT License** – veja o arquivo [LICENSE](./LICENSE) para mais detalhes.