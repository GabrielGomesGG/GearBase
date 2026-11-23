using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class OrdemServicoDAO : IDAO<OrdemServico>
    {
        public int Insert(OrdemServico ordemServico)
        {
            // Uma OS nova ainda não tem itens, então o valor das peças começa em zero
            ordemServico.ValorPecas = 0;
            ordemServico.CalcularValorTotal();
            ConferirRegras(ordemServico, true);

            string sql = @"INSERT INTO ordens_servico (cliente_id, veiculo_id, funcionario_id, data_abertura, data_conclusao, quilometragem_entrada, descricao_problema, diagnostico_tecnico, valor_mao_de_obra, valor_pecas, valor_total, forma_pagamento, status)
                           VALUES (@cliente_id, @veiculo_id, @funcionario_id, @data_abertura, @data_conclusao, @quilometragem_entrada, @descricao_problema, @diagnostico_tecnico, @valor_mao_de_obra, @valor_pecas, @valor_total, @forma_pagamento, @status)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", ordemServico.ClienteId);
                cmd.Parameters.AddWithValue("@veiculo_id", ordemServico.VeiculoId);
                cmd.Parameters.AddWithValue("@funcionario_id", ordemServico.FuncionarioId);
                cmd.Parameters.AddWithValue("@data_abertura", ordemServico.DataAbertura);
                cmd.Parameters.AddWithValue("@data_conclusao", BancoHelper.ParaBanco(ordemServico.DataConclusao));
                cmd.Parameters.AddWithValue("@quilometragem_entrada", ordemServico.QuilometragemEntrada);
                cmd.Parameters.AddWithValue("@descricao_problema", ordemServico.DescricaoProblema);
                cmd.Parameters.AddWithValue("@diagnostico_tecnico", BancoHelper.ParaBanco(ordemServico.DiagnosticoTecnico));
                cmd.Parameters.AddWithValue("@valor_mao_de_obra", ordemServico.ValorMaoDeObra);
                cmd.Parameters.AddWithValue("@valor_pecas", ordemServico.ValorPecas);
                cmd.Parameters.AddWithValue("@valor_total", ordemServico.ValorTotal);
                cmd.Parameters.AddWithValue("@forma_pagamento", BancoHelper.ParaBanco(ordemServico.FormaPagamento));
                cmd.Parameters.AddWithValue("@status", ordemServico.Status);
                cmd.ExecuteNonQuery();
                int id = (int)cmd.LastInsertedId;
                new LancamentoCaixaDAO().SincronizarOrdem(id);
                return id;
            }
        }

        public void Update(OrdemServico ordemServico)
        {
            ordemServico.CalcularValorTotal();
            ConferirRegras(ordemServico, false);

            string sql = @"UPDATE ordens_servico SET
                    cliente_id = @cliente_id,
                    veiculo_id = @veiculo_id,
                    funcionario_id = @funcionario_id,
                    data_abertura = @data_abertura,
                    data_conclusao = @data_conclusao,
                    quilometragem_entrada = @quilometragem_entrada,
                    descricao_problema = @descricao_problema,
                    diagnostico_tecnico = @diagnostico_tecnico,
                    valor_mao_de_obra = @valor_mao_de_obra,
                    valor_pecas = @valor_pecas,
                    valor_total = @valor_total,
                    forma_pagamento = @forma_pagamento,
                    status = @status
                    WHERE ordem_servico_id = @ordem_servico_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", ordemServico.ClienteId);
                cmd.Parameters.AddWithValue("@veiculo_id", ordemServico.VeiculoId);
                cmd.Parameters.AddWithValue("@funcionario_id", ordemServico.FuncionarioId);
                cmd.Parameters.AddWithValue("@data_abertura", ordemServico.DataAbertura);
                cmd.Parameters.AddWithValue("@data_conclusao", BancoHelper.ParaBanco(ordemServico.DataConclusao));
                cmd.Parameters.AddWithValue("@quilometragem_entrada", ordemServico.QuilometragemEntrada);
                cmd.Parameters.AddWithValue("@descricao_problema", ordemServico.DescricaoProblema);
                cmd.Parameters.AddWithValue("@diagnostico_tecnico", BancoHelper.ParaBanco(ordemServico.DiagnosticoTecnico));
                cmd.Parameters.AddWithValue("@valor_mao_de_obra", ordemServico.ValorMaoDeObra);
                cmd.Parameters.AddWithValue("@valor_pecas", ordemServico.ValorPecas);
                cmd.Parameters.AddWithValue("@valor_total", ordemServico.ValorTotal);
                cmd.Parameters.AddWithValue("@forma_pagamento", BancoHelper.ParaBanco(ordemServico.FormaPagamento));
                cmd.Parameters.AddWithValue("@status", ordemServico.Status);
                cmd.Parameters.AddWithValue("@ordem_servico_id", ordemServico.OrdemServicoId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma ordem de serviço encontrada com esse ID.");
            }

            RecalcularValores(ordemServico.OrdemServicoId);
        }

        /* O valor das peças sempre vem da soma dos itens da OS, e não do que foi enviado.
           Chamado aqui e pelo ItemOrdemServicoDAO sempre que um item muda. */
        public void RecalcularValores(int ordemServicoId)
        {
            // OS sem itens: o SUM devolve NULL, por isso o valor começa em zero
            decimal valorPecas = 0;
            string sqlSoma = "SELECT SUM(subtotal) AS total FROM itens_ordem_servico WHERE ordem_servico_id = @ordem_servico_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sqlSoma, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", ordemServicoId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal? soma = BancoHelper.DecimalOuNulo(reader, "total");
                        if (soma != null)
                            valorPecas = (decimal)soma;
                    }
                }
            }

            string sql = @"UPDATE ordens_servico SET
                    valor_pecas = @valor_pecas,
                    valor_total = valor_mao_de_obra + @valor_pecas
                    WHERE ordem_servico_id = @ordem_servico_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@valor_pecas", valorPecas);
                cmd.Parameters.AddWithValue("@ordem_servico_id", ordemServicoId);
                cmd.ExecuteNonQuery();
            }

            // O total mudou (ou a situação, no Update): o caixa acompanha
            new LancamentoCaixaDAO().SincronizarOrdem(ordemServicoId);
        }

        /* Quem abre a OS precisa ter permissão. O veículo precisa ser da frota do cliente ou da própria concessionária
           (PDS de veículo novo, revisão do carro de test drive) */
        private void ConferirRegras(OrdemServico ordemServico, bool cadastroNovo)
        {
            new FuncionarioDAO().ConferirPermissao(ordemServico.FuncionarioId, "pode_abrir_os", "abrir ordens de serviço", cadastroNovo);

            Veiculo veiculo = new VeiculoDAO().BuscarObrigatorio(ordemServico.VeiculoId);
            if (veiculo.ClienteId != null && veiculo.ClienteId != ordemServico.ClienteId)
                throw new Exception("Este veículo é de outro cliente. Confira o cliente ou o veículo escolhido.");
        }

        public void Delete(int id)
        {
            // Os itens e o lançamento do caixa apontam para a OS, então saem antes dela
            new LancamentoCaixaDAO().ApagarDaOrdem(id);

            string sqlItens = "DELETE FROM itens_ordem_servico WHERE ordem_servico_id = @ordem_servico_id";
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sqlItens, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", id);
                cmd.ExecuteNonQuery();
            }

            string sql = "DELETE FROM ordens_servico WHERE ordem_servico_id = @ordem_servico_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma ordem de serviço encontrada com esse ID.");
            }
        }

        public List<OrdemServico> ListarTodos()
        {
            string sql = "SELECT * FROM ordens_servico";
            List<OrdemServico> ordens = new List<OrdemServico>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    ordens.Add(LerLinha(reader));
            }
            return ordens;
        }

        public OrdemServico BuscarPorId(int id)
        {
            string sql = "SELECT * FROM ordens_servico WHERE ordem_servico_id = @ordem_servico_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto OrdemServico
        private OrdemServico LerLinha(MySqlDataReader reader)
        {
            return new OrdemServico
            {
                OrdemServicoId = Convert.ToInt32(reader["ordem_servico_id"]),
                ClienteId = Convert.ToInt32(reader["cliente_id"]),
                VeiculoId = Convert.ToInt32(reader["veiculo_id"]),
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                DataAbertura = Convert.ToDateTime(reader["data_abertura"]),
                DataConclusao = BancoHelper.DataHoraOuNula(reader, "data_conclusao"),
                QuilometragemEntrada = Convert.ToInt32(reader["quilometragem_entrada"]),
                DescricaoProblema = reader["descricao_problema"].ToString(),
                DiagnosticoTecnico = BancoHelper.TextoOuNulo(reader, "diagnostico_tecnico"),
                ValorMaoDeObra = Convert.ToDecimal(reader["valor_mao_de_obra"]),
                ValorPecas = Convert.ToDecimal(reader["valor_pecas"]),
                ValorTotal = Convert.ToDecimal(reader["valor_total"]),
                FormaPagamento = BancoHelper.TextoOuNulo(reader, "forma_pagamento"),
                Status = reader["status"].ToString()
            };
        }
    }
}
