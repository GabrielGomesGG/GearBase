USE gearBase;

-- JOIN

-- 1. Listar todas as vendas realizadas
SELECT
    v.venda_id,
    c.nome AS cliente_nome,
    m.nome AS modelo_veiculo,
    f.nome AS funcionario_vendedor,
    v.valor_total,
    v.data_venda
FROM vendas v
JOIN clientes c ON v.cliente_id = c.cliente_id
JOIN funcionarios f ON v.funcionario_id = f.funcionario_id
JOIN veiculos ve ON v.veiculo_id = ve.veiculo_id
JOIN modelos m ON ve.modelo_id = m.modelo_id;

-- 2. Relatório de Ordens de Serviço concluídas
SELECT
    os.ordem_servico_id,
    c.nome AS cliente_nome,
    ve.placa AS veiculo_placa,
    f.nome AS mecanico_responsavel,
    os.valor_total,
    os.data_conclusao
FROM ordens_servico os
JOIN clientes c ON os.cliente_id = c.cliente_id
JOIN veiculos ve ON os.veiculo_id = ve.veiculo_id
JOIN funcionarios f ON os.funcionario_id = f.funcionario_id
WHERE os.status = 'Concluído';

-- 3. Relação de Veículos e seus Opcionais instalados
SELECT
    v.veiculo_id,
    v.chassi,
    v.cor,
    o.nome AS opcional_nome,
    vo.preco_aplicado,
    vo.instalado_fabrica
FROM veiculos v
JOIN veiculo_opcionais vo ON v.veiculo_id = vo.veiculo_id
JOIN opcionais o ON vo.opcional_id = o.opcional_id;

-- 4. Listagem de Peças utilizadas em Ordens de Serviço
SELECT
    ios.ordem_servico_id,
    p.nome AS peca_nome,
    ios.quantidade,
    ios.preco_unitario,
    ios.subtotal,
    f.razao_social AS fornecedor_nome
FROM itens_ordem_servico ios
JOIN pecas p ON ios.peca_id = p.peca_id
JOIN fornecedores f ON p.fornecedor_id = f.fornecedor_id;

-- 5. Histórico de Test Drives agendados ou realizados
SELECT
    td.test_drive_id,
    c.nome AS cliente_nome,
    m.nome AS modelo_veiculo,
    f.nome AS funcionario_acompanhante,
    td.data_agendamento,
    td.status,
    td.avaliacao_cliente
FROM test_drives td
JOIN clientes c ON td.cliente_id = c.cliente_id
JOIN funcionarios f ON td.funcionario_id = f.funcionario_id
JOIN veiculos ve ON td.veiculo_id = ve.veiculo_id
JOIN modelos m ON ve.modelo_id = m.modelo_id;

-- GROUP BY

-- 1. Quantidade de veículos cadastrados agrupados por marca
SELECT
    mar.nome AS marca_nome,
    COUNT(v.veiculo_id) AS total_veiculos
FROM marcas mar
JOIN modelos mo ON mar.marca_id = mo.marca_id
JOIN veiculos v ON mo.modelo_id = v.modelo_id
GROUP BY mar.marca_id, mar.nome
HAVING COUNT(v.veiculo_id) > 1
ORDER BY total_veiculos DESC;

-- 2. Média de preço de venda dos veículos agrupados por categoria
SELECT
    mo.categoria,
    COUNT(v.veiculo_id) AS quantidade_veiculos,
    AVG(v.preco_venda) AS media_preco_venda
FROM modelos mo
JOIN veiculos v ON mo.modelo_id = v.modelo_id
GROUP BY mo.categoria
HAVING AVG(v.preco_venda) > 150000.00
ORDER BY AVG(v.preco_venda) DESC;

-- 3. Faturamento total de vendas concluídas agrupado por forma de pagamento
SELECT
    forma_pagamento,
    COUNT(venda_id) AS total_vendas_realizadas,
    SUM(valor_total) AS faturamento_total
FROM vendas
WHERE status_venda = 'Concluída'
GROUP BY forma_pagamento
ORDER BY SUM(valor_total) DESC;

-- 4. Quantidade de funcionários agrupados pelo cargo
SELECT
    c.nome AS cargo_nome,
    COUNT(f.funcionario_id) AS total_funcionarios
FROM cargos c
LEFT JOIN funcionarios f ON c.cargo_id = f.cargo_id
GROUP BY c.cargo_id, c.nome
ORDER BY total_funcionarios DESC;

-- 5. Total de peças em estoque agrupadas pelo fornecedor
SELECT
    f.razao_social AS fornecedor_nome,
    SUM(p.quantidade_estoque) AS total_pecas_estoque
FROM fornecedores f
JOIN pecas p ON f.fornecedor_id = p.fornecedor_id
GROUP BY f.fornecedor_id, f.razao_social
ORDER BY total_pecas_estoque DESC;

-- SUBCONSULTA

-- 1. Veículos com preço de venda acima da média geral de todos os veículos
SELECT
    v.veiculo_id,
    v.chassi,
    v.ano_fabricacao,
    v.preco_venda
FROM veiculos v
WHERE v.preco_venda > (SELECT AVG(preco_venda) FROM veiculos)
ORDER BY v.preco_venda ASC;

-- 2. Clientes que realizaram pelo menos uma compra de veículo na concessionária
SELECT
    c.cliente_id,
    c.nome,
    c.tipo_pessoa,
    c.telefone
FROM clientes c
WHERE c.cliente_id IN (SELECT cliente_id FROM vendas WHERE status_venda = 'Concluída')
ORDER BY c.nome ASC;

-- 3. Peças que possuem o preço de venda superior ao preço de venda médio de todas as peças
SELECT
    p.peca_id,
    p.codigo_referencia,
    p.nome,
    p.preco_venda
FROM pecas p
WHERE p.preco_venda > (SELECT AVG(preco_venda) FROM pecas)
ORDER BY p.preco_venda DESC;

-- 4. Funcionários que possuem o cargo com o maior salário base da concessionária
SELECT
    f.funcionario_id,
    f.nome AS funcionario_nome,
    f.email
FROM funcionarios f
WHERE f.cargo_id IN (
    SELECT cargo_id
    FROM cargos
    WHERE salario_base = (SELECT MAX(salario_base) FROM cargos)
);

-- 5. Veículos que nunca foram cadastrados em nenhum Test Drive
SELECT
    v.veiculo_id,
    v.chassi,
    v.cor,
    v.status
FROM veiculos v
WHERE v.veiculo_id NOT IN (SELECT veiculo_id FROM test_drives)
ORDER BY v.veiculo_id ASC;
