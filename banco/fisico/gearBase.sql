-- apaga o banco antigo, se existir, para criar tudo de novo
DROP DATABASE IF EXISTS gearBase;
CREATE DATABASE gearBase;

USE gearBase;

CREATE TABLE marcas (
    marca_id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL UNIQUE,
    pais_origem VARCHAR(50) NULL,
    site_oficial VARCHAR(100) NULL
);

CREATE TABLE modelos (
    modelo_id INT PRIMARY KEY AUTO_INCREMENT,
    marca_id INT NOT NULL,
    nome VARCHAR(50) NOT NULL,
    categoria ENUM('Sedan', 'Hatch', 'SUV', 'Picape', 'Coupé', 'Conversível', 'Van', 'Outro') NOT NULL,
    ano_lancamento INT NULL,
    FOREIGN KEY (marca_id) REFERENCES marcas (marca_id)
);

CREATE TABLE opcionais (
    opcional_id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL UNIQUE,
    descricao TEXT NULL,
    preco_tabela DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    categoria ENUM('Conforto', 'Segurança', 'Estética', 'Tecnologia', 'Desempenho') NULL
);

CREATE TABLE clientes (
    cliente_id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    tipo_pessoa ENUM('PF', 'PJ') NOT NULL DEFAULT 'PF',
    sexo ENUM('M', 'F', 'Outro') NULL,
    cpf_cnpj VARCHAR(18) NOT NULL UNIQUE,
    email VARCHAR(100) NULL UNIQUE,
    telefone VARCHAR(20) NOT NULL,
    data_nascimento DATE NULL,
    logradouro VARCHAR(150) NULL,
    numero VARCHAR(20) NULL,
    bairro VARCHAR(50) NULL,
    cidade VARCHAR(50) NULL,
    estado CHAR(2) NULL,
    cep VARCHAR(10) NULL,
    data_cadastro DATETIME NOT NULL
);

CREATE TABLE veiculos (
    veiculo_id INT PRIMARY KEY AUTO_INCREMENT,
    modelo_id INT NOT NULL,
    cliente_id INT NULL,
    chassi VARCHAR(17) NOT NULL UNIQUE,
    placa VARCHAR(10) NULL UNIQUE,
    ano_fabricacao INT NOT NULL,
    ano_modelo INT NOT NULL,
    cor VARCHAR(30) NOT NULL,
    combustivel ENUM('Gasolina', 'Etanol', 'Flex', 'Diesel', 'Elétrico', 'Híbrido') NOT NULL,
    transmissao ENUM('Manual', 'Automático', 'CVT', 'Automatizado') NOT NULL,
    motorizacao VARCHAR(20) NOT NULL,
    potencia_cv INT NULL,
    portas INT NOT NULL DEFAULT 4,
    quilometragem INT NOT NULL DEFAULT 0,
    preco_custo DECIMAL(10,2) NOT NULL,
    preco_venda DECIMAL(10,2) NOT NULL,
    tipo ENUM('Novo', 'Seminovo') NOT NULL,
    status ENUM('Disponível', 'Reservado', 'Vendido', 'Em Manutenção') NOT NULL DEFAULT 'Disponível',
    data_entrada DATE NOT NULL,
    FOREIGN KEY (modelo_id) REFERENCES modelos (modelo_id),
    FOREIGN KEY (cliente_id) REFERENCES clientes (cliente_id)
);

CREATE TABLE veiculo_opcionais (
    veiculo_id INT NOT NULL,
    opcional_id INT NOT NULL,
    preco_aplicado DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    instalado_fabrica BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (veiculo_id, opcional_id),
    FOREIGN KEY (veiculo_id) REFERENCES veiculos (veiculo_id),
    FOREIGN KEY (opcional_id) REFERENCES opcionais (opcional_id)
);

CREATE TABLE cargos (
    cargo_id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL UNIQUE,
    salario_base DECIMAL(10,2) NOT NULL,
    comissao_percentual DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    pode_vender BOOLEAN NOT NULL DEFAULT FALSE,
    pode_abrir_os BOOLEAN NOT NULL DEFAULT FALSE,
    pode_liberar BOOLEAN NOT NULL DEFAULT FALSE,
    pode_financeiro BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE funcionarios (
    funcionario_id INT PRIMARY KEY AUTO_INCREMENT,
    cargo_id INT NOT NULL,
    matricula VARCHAR(10) NOT NULL UNIQUE,
    senha_hash CHAR(64) NOT NULL,
    nome VARCHAR(100) NOT NULL,
    cpf VARCHAR(14) NOT NULL UNIQUE,
    sexo ENUM('M', 'F', 'Outro') NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    telefone VARCHAR(20) NOT NULL,
    data_nascimento DATE NULL,
    data_admissao DATE NOT NULL,
    data_demissao DATE NULL,
    status ENUM('Ativo', 'Férias', 'Afastado', 'Desligado') NOT NULL DEFAULT 'Ativo',
    FOREIGN KEY (cargo_id) REFERENCES cargos (cargo_id)
);

CREATE TABLE fornecedores (
    fornecedor_id INT PRIMARY KEY AUTO_INCREMENT,
    razao_social VARCHAR(100) NOT NULL,
    nome_fantasia VARCHAR(100) NULL,
    cnpj VARCHAR(18) NOT NULL UNIQUE,
    inscricao_estadual VARCHAR(30) NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    telefone VARCHAR(20) NOT NULL,
    contato_principal VARCHAR(100) NULL,
    endereco VARCHAR(200) NULL
);

CREATE TABLE vendas (
    venda_id INT PRIMARY KEY AUTO_INCREMENT,
    cliente_id INT NOT NULL,
    funcionario_id INT NOT NULL,
    veiculo_id INT NOT NULL,
    data_venda DATETIME NOT NULL,
    valor_veiculo DECIMAL(10,2) NOT NULL,
    valor_entrada DECIMAL(10,2) NULL DEFAULT 0.00,
    desconto DECIMAL(10,2) NULL DEFAULT 0.00,
    valor_total DECIMAL(10,2) NOT NULL,
    forma_pagamento ENUM('À Vista', 'Financiamento', 'Consórcio', 'Entrada + Financiamento', 'Cartão de Crédito') NOT NULL,
    quantidade_parcelas INT NULL DEFAULT 1,
    taxa_juros DECIMAL(5,2) NULL DEFAULT 0.00,
    status_venda ENUM('Pendente', 'Concluída', 'Cancelada') NOT NULL DEFAULT 'Concluída',
    observacoes TEXT NULL,
    FOREIGN KEY (cliente_id) REFERENCES clientes (cliente_id),
    FOREIGN KEY (funcionario_id) REFERENCES funcionarios (funcionario_id),
    FOREIGN KEY (veiculo_id) REFERENCES veiculos (veiculo_id)
);

CREATE TABLE test_drives (
    test_drive_id INT PRIMARY KEY AUTO_INCREMENT,
    cliente_id INT NOT NULL,
    veiculo_id INT NOT NULL,
    funcionario_id INT NOT NULL,
    data_agendamento DATETIME NOT NULL,
    cnh_cliente VARCHAR(20) NOT NULL,
    validade_cnh DATE NOT NULL,
    km_inicial INT NULL,
    km_final INT NULL,
    status ENUM('Agendado', 'Em Andamento', 'Realizado', 'Cancelado') NOT NULL DEFAULT 'Agendado',
    avaliacao_cliente INT NULL,
    observacoes TEXT NULL,
    FOREIGN KEY (cliente_id) REFERENCES clientes (cliente_id),
    FOREIGN KEY (veiculo_id) REFERENCES veiculos (veiculo_id),
    FOREIGN KEY (funcionario_id) REFERENCES funcionarios (funcionario_id)
);

CREATE TABLE pecas (
    peca_id INT PRIMARY KEY AUTO_INCREMENT,
    fornecedor_id INT NOT NULL,
    codigo_referencia VARCHAR(50) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    descricao TEXT NULL,
    preco_custo DECIMAL(10,2) NOT NULL,
    preco_venda DECIMAL(10,2) NOT NULL,
    quantidade_estoque INT NOT NULL DEFAULT 0,
    estoque_minimo INT NOT NULL DEFAULT 5,
    localizacao_prateleira VARCHAR(50) NULL,
    FOREIGN KEY (fornecedor_id) REFERENCES fornecedores (fornecedor_id)
);

CREATE TABLE ordens_servico (
    ordem_servico_id INT PRIMARY KEY AUTO_INCREMENT,
    cliente_id INT NOT NULL,
    veiculo_id INT NOT NULL,
    funcionario_id INT NOT NULL,
    data_abertura DATETIME NOT NULL,
    data_conclusao DATETIME NULL,
    quilometragem_entrada INT NOT NULL,
    descricao_problema TEXT NOT NULL,
    diagnostico_tecnico TEXT NULL,
    valor_mao_de_obra DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    valor_pecas DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    valor_total DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    forma_pagamento ENUM('À Vista', 'Cartão de Crédito', 'Faturado', 'Pix') NULL,
    status ENUM('Aberto', 'Em Andamento', 'Aguardando Peça', 'Concluído', 'Cancelado') NOT NULL DEFAULT 'Aberto',
    FOREIGN KEY (cliente_id) REFERENCES clientes (cliente_id),
    FOREIGN KEY (veiculo_id) REFERENCES veiculos (veiculo_id),
    FOREIGN KEY (funcionario_id) REFERENCES funcionarios (funcionario_id)
);

CREATE TABLE itens_ordem_servico (
    item_os_id INT PRIMARY KEY AUTO_INCREMENT,
    ordem_servico_id INT NOT NULL,
    peca_id INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    desconto DECIMAL(10,2) NULL DEFAULT 0.00,
    subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (ordem_servico_id) REFERENCES ordens_servico (ordem_servico_id),
    FOREIGN KEY (peca_id) REFERENCES pecas (peca_id)
);

CREATE TABLE lancamentos_caixa (
    lancamento_id INT PRIMARY KEY AUTO_INCREMENT,
    funcionario_id INT NOT NULL,
    venda_id INT NULL UNIQUE,
    ordem_servico_id INT NULL UNIQUE,
    tipo ENUM('Entrada', 'Saída') NOT NULL,
    categoria ENUM('Venda de veículo', 'Ordem de serviço', 'Compra de peças', 'Salários', 'Despesas fixas', 'Impostos', 'Outros') NOT NULL,
    descricao VARCHAR(150) NOT NULL,
    valor DECIMAL(10,2) NOT NULL,
    forma_pagamento ENUM('À Vista', 'Pix', 'Cartão de Crédito', 'Cartão de Débito', 'Boleto', 'Transferência', 'Financiamento', 'Consórcio', 'Entrada + Financiamento', 'Faturado') NOT NULL,
    data_lancamento DATETIME NOT NULL,
    FOREIGN KEY (funcionario_id) REFERENCES funcionarios (funcionario_id),
    FOREIGN KEY (venda_id) REFERENCES vendas (venda_id),
    FOREIGN KEY (ordem_servico_id) REFERENCES ordens_servico (ordem_servico_id)
);
