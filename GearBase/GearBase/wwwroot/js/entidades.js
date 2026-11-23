/* Cada tela do sistema é descrita aqui: rota da API, nomes para os textos e campos do formulário.
   O app.js monta a tabela e o formulário a partir disso, então todas as telas funcionam do mesmo jeito.

   Tipos de campo: texto, textoLongo, email, url, inteiro, dinheiro, percentual,
   data, dataHora, opcoes, referencia e booleano.
   "calculado" aparece na tabela mas não no formulário (a API calcula o valor).
   "encerrados" são as situações que vão para o fim da tabela (o que está em aberto aparece primeiro).
   "filtro" limita as opções de um campo de referência; "dependeDe" refaz as opções quando outro campo muda.
   A API confere as mesmas regras: o filtro só evita que o usuário escolha algo que vai ser recusado. */

const SEXO = ["M", "F", "Outro"];

// Funcionário ativo cujo cargo tem a permissão (podeVender ou podeAbrirOs)
const comPermissao = permissao => (funcionario, valores, listas) =>
    funcionario.status === "Ativo" &&
    Boolean(listas.cargos.find(c => c.cargoId === funcionario.cargoId)?.[permissao]);

// Veículo da concessionária (sem dono) e disponível
const doEstoque = veiculo => veiculo.clienteId === null && veiculo.status === "Disponível";

// Veículo da frota do cliente escolhido no formulário, ou da concessionária (PDS, revisão do carro de test drive)
const daFrotaDoCliente = (veiculo, valores) =>
    valores.clienteId != null && (veiculo.clienteId === valores.clienteId || veiculo.clienteId === null);

const entidades = {
    "marcas": {
        singular: "marca", plural: "marcas", feminino: true,
        id: "marcaId",
        descrever: r => r.nome,
        colunas: ["nome", "paisOrigem", "siteOficial"],
        campos: [
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 50 },
            { nome: "paisOrigem", rotulo: "País de origem", tipo: "texto", max: 50 },
            { nome: "siteOficial", rotulo: "Site oficial", tipo: "url", max: 100 }
        ]
    },

    "modelos": {
        singular: "modelo", plural: "modelos",
        id: "modeloId",
        descrever: r => r.nome,
        colunas: ["nome", "marcaId", "categoria", "anoLancamento"],
        campos: [
            { nome: "marcaId", rotulo: "Marca", tipo: "referencia", rota: "marcas", obrigatorio: true },
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 50 },
            { nome: "categoria", rotulo: "Categoria", tipo: "opcoes", obrigatorio: true,
              opcoes: ["Sedan", "Hatch", "SUV", "Picape", "Coupé", "Conversível", "Van", "Outro"] },
            { nome: "anoLancamento", rotulo: "Ano de lançamento", tipo: "inteiro", min: 1900, maxValor: 2100 }
        ]
    },

    "opcionais": {
        singular: "opcional", plural: "opcionais",
        id: "opcionalId",
        descrever: r => r.nome,
        colunas: ["nome", "categoria", "precoTabela"],
        campos: [
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 50 },
            { nome: "categoria", rotulo: "Categoria", tipo: "opcoes",
              opcoes: ["Conforto", "Segurança", "Estética", "Tecnologia", "Desempenho"] },
            { nome: "precoTabela", rotulo: "Preço de tabela", tipo: "dinheiro", obrigatorio: true },
            { nome: "descricao", rotulo: "Descrição", tipo: "textoLongo" }
        ]
    },

    "veiculos": {
        singular: "veículo", plural: "veículos",
        id: "veiculoId",
        descrever: r => r.placa ?? r.chassi,
        colunas: ["placa", "modeloId", "clienteId", "anoModelo", "cor", "quilometragem", "precoVenda", "status"],
        campos: [
            { nome: "modeloId", rotulo: "Modelo", tipo: "referencia", rota: "modelos", obrigatorio: true },
            // Sem dono, o veículo é da concessionária
            { nome: "clienteId", rotulo: "Proprietário", tipo: "referencia", rota: "clientes", vazio: "Concessionária" },
            { nome: "chassi", rotulo: "Chassi", tipo: "texto", obrigatorio: true, min: 17, max: 17 },
            { nome: "placa", rotulo: "Placa", tipo: "texto", max: 10 },
            { nome: "cor", rotulo: "Cor", tipo: "texto", obrigatorio: true, max: 30 },
            { nome: "anoFabricacao", rotulo: "Ano de fabricação", tipo: "inteiro", obrigatorio: true, min: 1900, maxValor: 2100 },
            { nome: "anoModelo", rotulo: "Ano do modelo", tipo: "inteiro", obrigatorio: true, min: 1900, maxValor: 2100 },
            { nome: "combustivel", rotulo: "Combustível", tipo: "opcoes", obrigatorio: true,
              opcoes: ["Gasolina", "Etanol", "Flex", "Diesel", "Elétrico", "Híbrido"] },
            { nome: "transmissao", rotulo: "Transmissão", tipo: "opcoes", obrigatorio: true,
              opcoes: ["Manual", "Automático", "CVT", "Automatizado"] },
            { nome: "motorizacao", rotulo: "Motorização", tipo: "texto", obrigatorio: true, max: 20 },
            { nome: "potenciaCv", rotulo: "Potência (cv)", tipo: "inteiro", min: 0 },
            { nome: "portas", rotulo: "Portas", tipo: "inteiro", obrigatorio: true, min: 1, padrao: 4 },
            { nome: "quilometragem", rotulo: "Quilometragem", tipo: "inteiro", obrigatorio: true, min: 0, padrao: 0, sufixo: " km" },
            { nome: "precoCusto", rotulo: "Preço de custo", tipo: "dinheiro", obrigatorio: true },
            { nome: "precoVenda", rotulo: "Preço de venda", tipo: "dinheiro", obrigatorio: true },
            { nome: "tipo", rotulo: "Tipo", tipo: "opcoes", obrigatorio: true, opcoes: ["Novo", "Seminovo"] },
            { nome: "status", rotulo: "Situação", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Disponível",
              opcoes: ["Disponível", "Reservado", "Vendido", "Em Manutenção"], encerrados: ["Vendido"] },
            { nome: "dataEntrada", rotulo: "Data de entrada", tipo: "data", obrigatorio: true }
        ]
    },

    "veiculo-opcionais": {
        singular: "opcional do veículo", plural: "opcionais instalados em veículos",
        // A tabela não tem ID próprio: o registro é achado pelo veículo e pelo opcional juntos
        chaveComposta: ["veiculoId", "opcionalId"],
        descrever: (r, nomeDe) => `${nomeDe("opcionalId", r.opcionalId)} no veículo ${nomeDe("veiculoId", r.veiculoId)}`,
        colunas: ["veiculoId", "opcionalId", "precoAplicado", "instaladoFabrica"],
        campos: [
            { nome: "veiculoId", rotulo: "Veículo", tipo: "referencia", rota: "veiculos", obrigatorio: true },
            { nome: "opcionalId", rotulo: "Opcional", tipo: "referencia", rota: "opcionais", obrigatorio: true },
            { nome: "precoAplicado", rotulo: "Preço aplicado", tipo: "dinheiro", obrigatorio: true },
            { nome: "instaladoFabrica", rotulo: "Instalado de fábrica", tipo: "booleano", padrao: true }
        ]
    },

    "vendas": {
        singular: "venda", plural: "vendas", feminino: true,
        id: "vendaId",
        descrever: r => `nº ${r.vendaId}`,
        colunas: ["vendaId", "dataVenda", "clienteId", "veiculoId", "funcionarioId", "valorTotal", "formaPagamento", "statusVenda"],
        campos: [
            { nome: "vendaId", rotulo: "Nº", tipo: "inteiro", calculado: true },
            { nome: "clienteId", rotulo: "Cliente", tipo: "referencia", rota: "clientes", obrigatorio: true },
            { nome: "veiculoId", rotulo: "Veículo", tipo: "referencia", rota: "veiculos", obrigatorio: true,
              filtro: doEstoque, textoSemOpcoes: "Nenhum veículo disponível no estoque" },
            { nome: "funcionarioId", rotulo: "Vendedor", tipo: "referencia", rota: "funcionarios", obrigatorio: true,
              filtro: comPermissao("podeVender"), usa: ["cargos"] },
            { nome: "dataVenda", rotulo: "Data da venda", tipo: "dataHora", obrigatorio: true },
            { nome: "valorVeiculo", rotulo: "Valor do veículo", tipo: "dinheiro", obrigatorio: true },
            { nome: "valorEntrada", rotulo: "Entrada", tipo: "dinheiro" },
            { nome: "desconto", rotulo: "Desconto", tipo: "dinheiro" },
            { nome: "valorTotal", rotulo: "Total", tipo: "dinheiro", calculado: true },
            { nome: "formaPagamento", rotulo: "Forma de pagamento", tipo: "opcoes", obrigatorio: true,
              opcoes: ["À Vista", "Financiamento", "Consórcio", "Entrada + Financiamento", "Cartão de Crédito"] },
            // Acima de 60 parcelas, um gerente precisa liberar (matrícula e senha), a não ser que o logado já seja gerente
            { nome: "quantidadeParcelas", rotulo: "Parcelas", tipo: "inteiro", min: 1, padrao: 1, limiteSemLiberacao: 60 },
            { nome: "taxaJuros", rotulo: "Juros ao mês", tipo: "percentual" },
            { nome: "statusVenda", rotulo: "Situação", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Concluída",
              opcoes: ["Pendente", "Concluída", "Cancelada"], encerrados: ["Concluída", "Cancelada"] },
            { nome: "observacoes", rotulo: "Observações", tipo: "textoLongo" }
        ]
    },

    "test-drives": {
        singular: "test drive", plural: "test drives",
        id: "testDriveId",
        descrever: (r, nomeDe) => `de ${nomeDe("clienteId", r.clienteId)}`,
        colunas: ["dataAgendamento", "clienteId", "veiculoId", "funcionarioId", "status", "avaliacaoCliente"],
        campos: [
            { nome: "clienteId", rotulo: "Cliente", tipo: "referencia", rota: "clientes", obrigatorio: true },
            { nome: "veiculoId", rotulo: "Veículo", tipo: "referencia", rota: "veiculos", obrigatorio: true,
              filtro: doEstoque, textoSemOpcoes: "Nenhum veículo disponível no estoque" },
            { nome: "funcionarioId", rotulo: "Acompanhante", tipo: "referencia", rota: "funcionarios", obrigatorio: true,
              filtro: comPermissao("podeVender"), usa: ["cargos"] },
            { nome: "dataAgendamento", rotulo: "Data e hora", tipo: "dataHora", obrigatorio: true },
            { nome: "cnhCliente", rotulo: "CNH do cliente", tipo: "texto", obrigatorio: true, max: 20 },
            { nome: "validadeCnh", rotulo: "Validade da CNH", tipo: "data", obrigatorio: true },
            { nome: "kmInicial", rotulo: "Km inicial", tipo: "inteiro", min: 0 },
            { nome: "kmFinal", rotulo: "Km final", tipo: "inteiro", min: 0 },
            { nome: "status", rotulo: "Situação", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Agendado",
              opcoes: ["Agendado", "Em Andamento", "Realizado", "Cancelado"], encerrados: ["Realizado", "Cancelado"] },
            { nome: "avaliacaoCliente", rotulo: "Avaliação (1 a 5)", tipo: "inteiro", min: 1, maxValor: 5 },
            { nome: "observacoes", rotulo: "Observações", tipo: "textoLongo" }
        ]
    },

    "clientes": {
        singular: "cliente", plural: "clientes",
        id: "clienteId",
        descrever: r => r.nome,
        colunas: ["nome", "tipoPessoa", "cpfCnpj", "telefone", "cidade"],
        campos: [
            { nome: "nome", rotulo: "Nome ou razão social", tipo: "texto", obrigatorio: true, max: 100 },
            { nome: "tipoPessoa", rotulo: "Tipo", tipo: "opcoes", obrigatorio: true, padrao: "PF", opcoes: ["PF", "PJ"] },
            { nome: "cpfCnpj", rotulo: "CPF ou CNPJ", tipo: "texto", obrigatorio: true, max: 18 },
            { nome: "sexo", rotulo: "Sexo", tipo: "opcoes", opcoes: SEXO },
            { nome: "dataNascimento", rotulo: "Data de nascimento", tipo: "data" },
            { nome: "telefone", rotulo: "Telefone", tipo: "texto", obrigatorio: true, max: 20 },
            { nome: "email", rotulo: "E-mail", tipo: "email", max: 100 },
            { nome: "cep", rotulo: "CEP", tipo: "texto", max: 10 },
            { nome: "logradouro", rotulo: "Logradouro", tipo: "texto", max: 150 },
            { nome: "numero", rotulo: "Número", tipo: "texto", max: 20 },
            { nome: "bairro", rotulo: "Bairro", tipo: "texto", max: 50 },
            { nome: "cidade", rotulo: "Cidade", tipo: "texto", max: 50 },
            { nome: "estado", rotulo: "UF", tipo: "texto", min: 2, max: 2 }
        ]
    },

    "ordens-servico": {
        singular: "ordem de serviço", plural: "ordens de serviço", feminino: true,
        id: "ordemServicoId",
        descrever: r => `nº ${r.ordemServicoId}`,
        colunas: ["ordemServicoId", "dataAbertura", "clienteId", "veiculoId", "valorTotal", "status"],
        campos: [
            { nome: "ordemServicoId", rotulo: "Nº", tipo: "inteiro", calculado: true },
            { nome: "clienteId", rotulo: "Cliente", tipo: "referencia", rota: "clientes", obrigatorio: true },
            { nome: "veiculoId", rotulo: "Veículo", tipo: "referencia", rota: "veiculos", obrigatorio: true,
              filtro: daFrotaDoCliente, dependeDe: "clienteId",
              textoSemOrigem: "Escolha o cliente primeiro", textoSemOpcoes: "Este cliente não tem veículos na frota" },
            { nome: "funcionarioId", rotulo: "Responsável", tipo: "referencia", rota: "funcionarios", obrigatorio: true,
              filtro: comPermissao("podeAbrirOs"), usa: ["cargos"] },
            { nome: "dataAbertura", rotulo: "Abertura", tipo: "dataHora", obrigatorio: true },
            { nome: "dataConclusao", rotulo: "Conclusão", tipo: "dataHora" },
            { nome: "quilometragemEntrada", rotulo: "Km na entrada", tipo: "inteiro", obrigatorio: true, min: 0, sufixo: " km" },
            { nome: "valorMaoDeObra", rotulo: "Mão de obra", tipo: "dinheiro", obrigatorio: true, padrao: 0 },
            { nome: "valorPecas", rotulo: "Peças", tipo: "dinheiro", calculado: true },
            { nome: "valorTotal", rotulo: "Total", tipo: "dinheiro", calculado: true },
            { nome: "formaPagamento", rotulo: "Forma de pagamento", tipo: "opcoes",
              opcoes: ["À Vista", "Cartão de Crédito", "Faturado", "Pix"] },
            { nome: "status", rotulo: "Situação", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Aberto",
              opcoes: ["Aberto", "Em Andamento", "Aguardando Peça", "Concluído", "Cancelado"], encerrados: ["Concluído", "Cancelado"] },
            { nome: "descricaoProblema", rotulo: "Problema relatado", tipo: "textoLongo", obrigatorio: true },
            { nome: "diagnosticoTecnico", rotulo: "Diagnóstico", tipo: "textoLongo" }
        ]
    },

    "itens-ordem-servico": {
        singular: "peça da OS", plural: "peças usadas nas ordens de serviço", feminino: true,
        id: "itemOsId",
        descrever: (r, nomeDe) => `${nomeDe("pecaId", r.pecaId)} da OS nº ${r.ordemServicoId}`,
        colunas: ["ordemServicoId", "pecaId", "quantidade", "precoUnitario", "desconto", "subtotal"],
        campos: [
            { nome: "ordemServicoId", rotulo: "Ordem de serviço", tipo: "referencia", rota: "ordens-servico", obrigatorio: true },
            { nome: "pecaId", rotulo: "Peça", tipo: "referencia", rota: "pecas", obrigatorio: true },
            { nome: "quantidade", rotulo: "Quantidade", tipo: "inteiro", obrigatorio: true, min: 1, padrao: 1 },
            { nome: "precoUnitario", rotulo: "Preço unitário", tipo: "dinheiro", obrigatorio: true },
            { nome: "desconto", rotulo: "Desconto", tipo: "dinheiro" },
            { nome: "subtotal", rotulo: "Subtotal", tipo: "dinheiro", calculado: true }
        ]
    },

    "pecas": {
        singular: "peça", plural: "peças", feminino: true,
        id: "pecaId",
        descrever: r => r.nome,
        colunas: ["codigoReferencia", "nome", "fornecedorId", "precoVenda", "quantidadeEstoque", "localizacaoPrateleira"],
        campos: [
            { nome: "codigoReferencia", rotulo: "Código", tipo: "texto", obrigatorio: true, max: 50 },
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 100 },
            { nome: "fornecedorId", rotulo: "Fornecedor", tipo: "referencia", rota: "fornecedores", obrigatorio: true },
            { nome: "localizacaoPrateleira", rotulo: "Prateleira", tipo: "texto", max: 50 },
            { nome: "precoCusto", rotulo: "Preço de custo", tipo: "dinheiro", obrigatorio: true },
            { nome: "precoVenda", rotulo: "Preço de venda", tipo: "dinheiro", obrigatorio: true },
            { nome: "quantidadeEstoque", rotulo: "Em estoque", tipo: "inteiro", obrigatorio: true, min: 0, padrao: 0,
              alerta: r => r.quantidadeEstoque < r.estoqueMinimo ? `mínimo ${r.estoqueMinimo}` : null },
            { nome: "estoqueMinimo", rotulo: "Estoque mínimo", tipo: "inteiro", obrigatorio: true, min: 0, padrao: 5 },
            { nome: "descricao", rotulo: "Descrição", tipo: "textoLongo" }
        ]
    },

    "fornecedores": {
        singular: "fornecedor", plural: "fornecedores",
        id: "fornecedorId",
        descrever: r => r.nomeFantasia ?? r.razaoSocial,
        colunas: ["razaoSocial", "nomeFantasia", "cnpj", "telefone", "contatoPrincipal"],
        campos: [
            { nome: "razaoSocial", rotulo: "Razão social", tipo: "texto", obrigatorio: true, max: 100 },
            { nome: "nomeFantasia", rotulo: "Nome fantasia", tipo: "texto", max: 100 },
            { nome: "cnpj", rotulo: "CNPJ", tipo: "texto", obrigatorio: true, max: 18 },
            { nome: "inscricaoEstadual", rotulo: "Inscrição estadual", tipo: "texto", max: 30 },
            { nome: "email", rotulo: "E-mail", tipo: "email", obrigatorio: true, max: 100 },
            { nome: "telefone", rotulo: "Telefone", tipo: "texto", obrigatorio: true, max: 20 },
            { nome: "contatoPrincipal", rotulo: "Contato", tipo: "texto", max: 100 },
            { nome: "endereco", rotulo: "Endereço", tipo: "texto", max: 200 }
        ]
    },

    "funcionarios": {
        singular: "funcionário", plural: "funcionários",
        id: "funcionarioId",
        descrever: r => r.nome,
        colunas: ["matricula", "nome", "cargoId", "telefone", "email", "dataAdmissao", "status"],
        campos: [
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 100 },
            // Login do sistema; funcionário novo entra com a senha padrão de demonstração
            { nome: "matricula", rotulo: "Matrícula", tipo: "texto", obrigatorio: true, max: 10 },
            { nome: "cargoId", rotulo: "Cargo", tipo: "referencia", rota: "cargos", obrigatorio: true },
            { nome: "cpf", rotulo: "CPF", tipo: "texto", obrigatorio: true, max: 14 },
            { nome: "sexo", rotulo: "Sexo", tipo: "opcoes", opcoes: SEXO },
            { nome: "dataNascimento", rotulo: "Data de nascimento", tipo: "data" },
            { nome: "telefone", rotulo: "Telefone", tipo: "texto", obrigatorio: true, max: 20 },
            { nome: "email", rotulo: "E-mail", tipo: "email", obrigatorio: true, max: 100 },
            { nome: "dataAdmissao", rotulo: "Admissão", tipo: "data", obrigatorio: true },
            { nome: "dataDemissao", rotulo: "Demissão", tipo: "data" },
            { nome: "status", rotulo: "Situação", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Ativo",
              opcoes: ["Ativo", "Férias", "Afastado", "Desligado"], encerrados: ["Desligado"] }
        ]
    },

    "caixa": {
        singular: "lançamento", plural: "lançamentos do caixa",
        id: "lancamentoId",
        descrever: r => `nº ${r.lancamentoId}`,
        // Do mais novo para o mais antigo, como um extrato
        ordenar: (a, b) => b.dataLancamento.localeCompare(a.dataLancamento),
        resumir: registros => {
            const somar = tipo => registros.filter(r => r.tipo === tipo).reduce((total, r) => total + r.valor, 0);
            const entradas = somar("Entrada");
            const saidas = somar("Saída");
            return `Entradas ${moeda.format(entradas)}, saídas ${moeda.format(saidas)}, saldo ${moeda.format(entradas - saidas)}.`;
        },
        // Lançamentos de venda e de OS são feitos pelo sistema: para mudar, altera-se a venda ou a OS
        automatico: r => r.vendaId ? "Pela venda" : r.ordemServicoId ? "Pela OS" : null,
        colunas: ["dataLancamento", "tipo", "categoria", "descricao", "valor", "formaPagamento", "origem"],
        campos: [
            { nome: "dataLancamento", rotulo: "Data e hora", tipo: "dataHora", obrigatorio: true },
            { nome: "tipo", rotulo: "Tipo", tipo: "opcoes", obrigatorio: true, situacao: true, padrao: "Saída",
              opcoes: ["Entrada", "Saída"] },
            { nome: "categoria", rotulo: "Categoria", tipo: "opcoes", obrigatorio: true,
              opcoes: ["Venda de veículo", "Ordem de serviço", "Compra de peças", "Salários", "Despesas fixas", "Impostos", "Outros"] },
            { nome: "valor", rotulo: "Valor", tipo: "dinheiro", obrigatorio: true },
            { nome: "formaPagamento", rotulo: "Forma de pagamento", tipo: "opcoes", obrigatorio: true,
              opcoes: ["À Vista", "Pix", "Cartão de Crédito", "Cartão de Débito", "Boleto", "Transferência",
                       "Financiamento", "Consórcio", "Entrada + Financiamento", "Faturado"] },
            { nome: "funcionarioId", rotulo: "Lançado por", tipo: "referencia", rota: "funcionarios", obrigatorio: true,
              filtro: comPermissao("podeFinanceiro"), usa: ["cargos"] },
            { nome: "descricao", rotulo: "Descrição", tipo: "texto", obrigatorio: true, max: 150 },
            { nome: "origem", rotulo: "Origem", calculado: true,
              exibir: r => r.vendaId ? `Venda nº ${r.vendaId}` : r.ordemServicoId ? `OS nº ${r.ordemServicoId}` : "Manual" }
        ]
    },

    "cargos": {
        singular: "cargo", plural: "cargos",
        id: "cargoId",
        descrever: r => r.nome,
        colunas: ["nome", "salarioBase", "comissaoPercentual", "podeVender", "podeAbrirOs", "podeLiberar", "podeFinanceiro"],
        campos: [
            { nome: "nome", rotulo: "Nome", tipo: "texto", obrigatorio: true, max: 50 },
            { nome: "salarioBase", rotulo: "Salário base", tipo: "dinheiro", obrigatorio: true },
            { nome: "comissaoPercentual", rotulo: "Comissão", tipo: "percentual", obrigatorio: true, padrao: 0 },
            { nome: "podeVender", rotulo: "Vende e acompanha test drive", tipo: "booleano", padrao: false },
            { nome: "podeAbrirOs", rotulo: "Abre ordem de serviço", tipo: "booleano", padrao: false },
            { nome: "podeLiberar", rotulo: "Libera parcelamento acima de 60x", tipo: "booleano", padrao: false },
            { nome: "podeFinanceiro", rotulo: "Lança no caixa", tipo: "booleano", padrao: false }
        ]
    }
};
