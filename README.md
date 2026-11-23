# GearBase

Sistema de gestão de uma concessionária de veículos com oficina: estoque de veículos, vendas, test drives, ordens de serviço, estoque de peças, caixa e equipe. É o trabalho das disciplinas de Banco de Dados II e Programação Orientada a Objetos (ADS, IFRO Campus Ji-Paraná), escrito para aprender e para ser lido. Não foi feito para uso em produção.

## O que tem aqui

| Arquivo / pasta | Conteúdo |
|---|---|
| `banco/conceitual/Concessionaria_conceitual.brM3` | Modelo conceitual no brModelo (entidades, relacionamentos e cardinalidades) |
| `banco/logico/Concessionaria_logico.brM3` | Modelo lógico no brModelo (tabelas, chaves primárias e estrangeiras) |
| `banco/fisico/gearBase.sql` | Criação do banco MySQL (15 tabelas, em 3FN) |
| `banco/fisico/gearBase_INSERTS.sql` | Dados de exemplo (cerca de 50 registros por tabela) |
| `banco/fisico/gearBase_SELECT.sql` | Consultas com JOIN, GROUP BY (2 delas com HAVING) e subconsulta, 5 de cada tipo |
| `GearBase/` | Back-end em C# (.NET 10, ASP.NET Core Web API), com ADO.NET e padrão DAO, e front-end em HTML, CSS e JavaScript puro |

Estrutura do projeto C#:

- `DataBase/`: conexão com o MySQL e conversão entre o `NULL` do banco e o `null` do C#
- `Mapeamento/`: uma classe para cada tabela, com encapsulamento das regras de valor (preço, quantidade, avaliação)
- `Interface/`: contratos dos DAOs (`IDAO<T>` e interfaces específicas)
- `DAO/`: acesso ao banco (Insert, Update, Delete, ListarTodos, BuscarPorId)
- `Controllers/`: endpoints da API (GET, POST, PUT, DELETE) para as 15 tabelas e o login
- `Program.cs`: configuração da API, Swagger, login com cookie, tratamento de erros e entrega do front-end
- `wwwroot/`: front-end, sem framework nem biblioteca
  - `login.html` e `js/login.js`: tela de entrada (matrícula e senha)
  - `index.html`: estrutura da página (menu, tabela e as janelas de formulário e de exclusão)
  - `css/estilo.css`: estilo, com as cores em variáveis
  - `js/api.js`: chamadas à API
  - `js/entidades.js`: descrição de cada tela (rota, campos, tipos e textos)
  - `js/app.js`: monta a tabela e o formulário de qualquer tela a partir do `entidades.js`

## Tabelas

`marcas`, `modelos`, `opcionais`, `veiculos`, `veiculo_opcionais`, `cargos`, `funcionarios`, `clientes`, `fornecedores`, `vendas`, `test_drives`, `pecas`, `ordens_servico`, `itens_ordem_servico` e `lancamentos_caixa`.

## Como rodar

1. Crie o banco no MySQL: rode `banco/fisico/gearBase.sql` e depois `banco/fisico/gearBase_INSERTS.sql`.
2. Crie o arquivo `GearBase/GearBase/DataBase/Conexao.cs` (veja abaixo).
3. Abra `GearBase/GearBase.slnx` no Visual Studio e execute (F5).
4. O navegador abre o sistema em `http://localhost:5000`. A API e o front sobem juntos.
5. Entre com uma matrícula e a senha de demonstração `gearbase` (ex.: `GBGG`, Gabriel Gomes, Gerente Comercial).
6. Para testar a API sem o front, use o Swagger em `http://localhost:5000/swagger`. Entre antes pelo sistema, no mesmo navegador: a API só responde para quem fez login.

## Regras do negócio

### Login

Cada funcionário tem uma matrícula formada por `GB`, a inicial do nome e a inicial de cada sobrenome: Gabriel Gomes vira `GBGG` e Carlos Eduardo Mendes vira `GBCEM`. As partículas "da", "de", "do", "das", "dos" e "e" não entram, e se duas pessoas ficarem com a mesma matrícula a segunda ganha um número (`GBCEM2`). A senha fica no banco como hash SHA-256; todos começam com `gearbase`, só para demonstração. Funcionário desligado não entra.

### Permissões por cargo

A tabela `cargos` diz quem vende (`pode_vender`), quem abre ordem de serviço (`pode_abrir_os`), quem libera parcelamento (`pode_liberar`) e quem lança no caixa (`pode_financeiro`). Vendas e test drives só aceitam funcionários que vendem, e ordens de serviço só aceitam o comercial e a recepção da oficina. Mecânicos e o TI não aparecem nessas listas, e a API recusa se alguém tentar.

### Frota e estoque

Cada veículo tem um dono (`veiculos.cliente_id`); sem dono, ele é da concessionária. A ordem de serviço aceita veículo da frota do cliente ou da própria concessionária (PDS de veículo novo, revisão do carro de test drive). Venda e test drive em aberto só usam veículo disponível no estoque. A venda concluída passa o veículo para o cliente, a pendente deixa reservado e a cancelada ou excluída devolve ao estoque.

### Parcelamento

Até 60 parcelas a venda é livre. Acima disso, um gerente (Gerente Comercial, Diretor de Operações, Coordenador de F&I ou Gerente de Seminovos) digita a matrícula e a senha para liberar; se quem está logado já é gerente, não pede senha. Quem liberou fica registrado nas observações da venda.

### Caixa

A tela Caixa mostra as entradas e saídas, do mais novo para o mais antigo, com o saldo no topo. Venda e OS concluídas entram sozinhas como entrada e saem se a venda for cancelada ou a OS reaberta; esses lançamentos só mudam pela venda ou pela OS. Os outros (salários, compras de peças, despesas) são lançados à mão por quem tem `pode_financeiro`.

Nos formulários, os campos de dinheiro e percentual são digitados como numa maquininha de cartão: os números entram pela direita, sempre com duas casas depois da vírgula.

## Por que o Conexao.cs não está no repositório

Esse arquivo guarda a senha do MySQL. Por isso ele está no `.gitignore` e não é enviado para o GitHub. Sem ele, o projeto não compila. Crie o arquivo com este conteúdo e troque `SUA_SENHA` pela senha do seu MySQL:

```csharp
using MySql.Data.MySqlClient;

namespace GearBase.DataBase
{
    internal static class Conexao
    {
        private const string strconexao =
            "server=localhost;uid=root;pwd=SUA_SENHA;database=gearBase;port=3306";

        public static MySqlConnection Conectar()
        {
            MySqlConnection conexao = new MySqlConnection(strconexao);
            conexao.Open();
            return conexao;
        }
    }
}
```

## Endpoints

Cada tabela com chave simples tem os mesmos cinco endpoints. Exemplo com marcas:

| Verbo | Rota | O que faz |
|---|---|---|
| GET | `/api/marcas` | Lista todas |
| GET | `/api/marcas/{id}` | Busca pelo ID |
| POST | `/api/marcas` | Cadastra |
| PUT | `/api/marcas/{id}` | Atualiza |
| DELETE | `/api/marcas/{id}` | Exclui |

Rotas das outras tabelas: `/api/modelos`, `/api/opcionais`, `/api/veiculos`, `/api/cargos`, `/api/funcionarios`, `/api/clientes`, `/api/fornecedores`, `/api/vendas`, `/api/test-drives`, `/api/pecas`, `/api/ordens-servico` e `/api/itens-ordem-servico`.

Rotas especiais:

- `/api/caixa`: lançamentos do caixa (CRUD; os automáticos não podem ser alterados nem excluídos).
- `/api/login`: `POST` entra (matrícula e senha), `GET` diz quem está logado e `DELETE` sai.
- `/api/veiculo-opcionais/{veiculoId}/{opcionalId}`: a tabela tem chave composta, então as rotas recebem os dois IDs.
- `/api/veiculo-opcionais/veiculo/{veiculoId}`: opcionais de um veículo.
- `/api/itens-ordem-servico/ordem/{ordemServicoId}`: itens de uma ordem de serviço.

## Conceitos de POO aplicados

- Encapsulamento: as propriedades com `get`/`set` recusam valores inválidos, como preço negativo, quantidade zero e avaliação fora de 1 a 5.
- Herança e classe abstrata: os controllers herdam de `CrudController<T>`, que escreve o CRUD uma vez só.
- Polimorfismo: cada controller implementa `DefinirId` do seu jeito, e o `CrudController` usa o DAO pela interface `IDAO<T>`.
- Interfaces e generics: `IDAO<T>` serve para todas as tabelas, e `IItemOrdemServicoDAO` herda dela e acrescenta um método.
- Regras de negócio nas classes: `Venda.CalcularValorTotal()`, `OrdemServico.CalcularValorTotal()` e `ItemOrdemServico.CalcularSubtotal()`.

## Sobre os comentários

O `gearBase.sql` começa com `DROP DATABASE IF EXISTS gearBase`: rodar esse script apaga o banco inteiro e cria tudo de novo.

No C#, os comentários explicam o porquê dos trechos que não ficam claros só lendo o código, como a conversão do `null` para `DBNull.Value` e o motivo de `veiculo_opcionais` não usar a interface genérica (chave composta). Os recursos que não vimos em sala (o login com cookie, o hash da senha com `SHA2`, o `LastInsertedId` e o tratamento único de erros, entre outros) têm um comentário dizendo isso e explicando por que foram usados.
