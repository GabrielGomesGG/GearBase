using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class LancamentoCaixaDAO : IDAO<LancamentoCaixa>
    {
        // Lançamento feito à mão pelo financeiro. Os de venda e de OS são feitos pelo próprio sistema (veja Sincronizar)
        public int Insert(LancamentoCaixa lancamento)
        {
            new FuncionarioDAO().ConferirPermissao(lancamento.FuncionarioId, "pode_financeiro", "fazer lançamentos no caixa", true);

            // Lançamento manual não tem venda nem OS
            lancamento.VendaId = null;
            lancamento.OrdemServicoId = null;
            return Gravar(lancamento);
        }

        public void Update(LancamentoCaixa lancamento)
        {
            ConferirManual(lancamento.LancamentoId);
            new FuncionarioDAO().ConferirPermissao(lancamento.FuncionarioId, "pode_financeiro", "fazer lançamentos no caixa", false);

            string sql = @"UPDATE lancamentos_caixa SET
                    funcionario_id = @funcionario_id,
                    tipo = @tipo,
                    categoria = @categoria,
                    descricao = @descricao,
                    valor = @valor,
                    forma_pagamento = @forma_pagamento,
                    data_lancamento = @data_lancamento
                    WHERE lancamento_id = @lancamento_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", lancamento.FuncionarioId);
                cmd.Parameters.AddWithValue("@tipo", lancamento.Tipo);
                cmd.Parameters.AddWithValue("@categoria", lancamento.Categoria);
                cmd.Parameters.AddWithValue("@descricao", lancamento.Descricao);
                cmd.Parameters.AddWithValue("@valor", lancamento.Valor);
                cmd.Parameters.AddWithValue("@forma_pagamento", lancamento.FormaPagamento);
                cmd.Parameters.AddWithValue("@data_lancamento", lancamento.DataLancamento);
                cmd.Parameters.AddWithValue("@lancamento_id", lancamento.LancamentoId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum lançamento encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            ConferirManual(id);
            string sql = "DELETE FROM lancamentos_caixa WHERE lancamento_id = @lancamento_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@lancamento_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum lançamento encontrado com esse ID.");
            }
        }

        public List<LancamentoCaixa> ListarTodos()
        {
            string sql = "SELECT * FROM lancamentos_caixa";
            List<LancamentoCaixa> lancamentos = new List<LancamentoCaixa>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lancamentos.Add(LerLinha(reader));
            }
            return lancamentos;
        }

        public LancamentoCaixa BuscarPorId(int id)
        {
            string sql = "SELECT * FROM lancamentos_caixa WHERE lancamento_id = @lancamento_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@lancamento_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Lançamento de venda ou de OS não é alterado à mão: quem manda nele é a venda ou a OS
        private void ConferirManual(int id)
        {
            LancamentoCaixa lancamento = BuscarPorId(id);
            if (lancamento == null)
                throw new Exception("Nenhum lançamento encontrado com esse ID.");
            if (lancamento.VendaId != null)
                throw new Exception($"Este lançamento veio da venda nº {lancamento.VendaId}. Para mudar, altere a venda.");
            if (lancamento.OrdemServicoId != null)
                throw new Exception($"Este lançamento veio da OS nº {lancamento.OrdemServicoId}. Para mudar, altere a OS.");
        }

        /* Refaz o lançamento de uma venda: apaga o que havia e, se a venda está concluída, lança o total como entrada.
           Assim o caixa acompanha qualquer mudança (concluir, cancelar, mudar o valor). */
        public void SincronizarVenda(int vendaId)
        {
            ApagarDaVenda(vendaId);

            Venda venda = new VendaDAO().BuscarPorId(vendaId);
            if (venda == null || venda.StatusVenda != "Concluída" || venda.ValorTotal <= 0)
                return;

            LancamentoCaixa lancamento = new LancamentoCaixa();
            lancamento.FuncionarioId = venda.FuncionarioId;
            lancamento.VendaId = venda.VendaId;
            lancamento.Tipo = "Entrada";
            lancamento.Categoria = "Venda de veículo";
            lancamento.Descricao = "Venda nº " + venda.VendaId;
            lancamento.Valor = venda.ValorTotal;
            lancamento.FormaPagamento = venda.FormaPagamento;
            lancamento.DataLancamento = venda.DataVenda;
            Gravar(lancamento);
        }

        // O mesmo para a OS: só a concluída entra no caixa, com o valor total (mão de obra + peças)
        public void SincronizarOrdem(int ordemServicoId)
        {
            ApagarDaOrdem(ordemServicoId);

            OrdemServico ordem = new OrdemServicoDAO().BuscarPorId(ordemServicoId);
            if (ordem == null || ordem.Status != "Concluído" || ordem.ValorTotal <= 0)
                return;

            LancamentoCaixa lancamento = new LancamentoCaixa();
            lancamento.FuncionarioId = ordem.FuncionarioId;
            lancamento.OrdemServicoId = ordem.OrdemServicoId;
            lancamento.Tipo = "Entrada";
            lancamento.Categoria = "Ordem de serviço";
            lancamento.Descricao = "OS nº " + ordem.OrdemServicoId;
            lancamento.Valor = ordem.ValorTotal;

            // A OS pode estar sem forma de pagamento e sem data de conclusão, mas o caixa precisa dos dois
            lancamento.FormaPagamento = "À Vista";
            if (ordem.FormaPagamento != null)
                lancamento.FormaPagamento = ordem.FormaPagamento;

            lancamento.DataLancamento = ordem.DataAbertura;
            if (ordem.DataConclusao != null)
                lancamento.DataLancamento = (DateTime)ordem.DataConclusao;

            Gravar(lancamento);
        }

        public void ApagarDaVenda(int vendaId)
        {
            Executar("DELETE FROM lancamentos_caixa WHERE venda_id = @id", vendaId);
        }

        public void ApagarDaOrdem(int ordemServicoId)
        {
            Executar("DELETE FROM lancamentos_caixa WHERE ordem_servico_id = @id", ordemServicoId);
        }

        private void Executar(string sql, int id)
        {
            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // INSERT usado pelo lançamento manual e pelos automáticos
        private int Gravar(LancamentoCaixa lancamento)
        {
            string sql = @"INSERT INTO lancamentos_caixa (funcionario_id, venda_id, ordem_servico_id, tipo, categoria, descricao, valor, forma_pagamento, data_lancamento)
                           VALUES (@funcionario_id, @venda_id, @ordem_servico_id, @tipo, @categoria, @descricao, @valor, @forma_pagamento, @data_lancamento)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", lancamento.FuncionarioId);
                cmd.Parameters.AddWithValue("@venda_id", BancoHelper.ParaBanco(lancamento.VendaId));
                cmd.Parameters.AddWithValue("@ordem_servico_id", BancoHelper.ParaBanco(lancamento.OrdemServicoId));
                cmd.Parameters.AddWithValue("@tipo", lancamento.Tipo);
                cmd.Parameters.AddWithValue("@categoria", lancamento.Categoria);
                cmd.Parameters.AddWithValue("@descricao", lancamento.Descricao);
                cmd.Parameters.AddWithValue("@valor", lancamento.Valor);
                cmd.Parameters.AddWithValue("@forma_pagamento", lancamento.FormaPagamento);
                cmd.Parameters.AddWithValue("@data_lancamento", lancamento.DataLancamento);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        // Converte uma linha do banco em um objeto LancamentoCaixa
        private LancamentoCaixa LerLinha(MySqlDataReader reader)
        {
            return new LancamentoCaixa
            {
                LancamentoId = Convert.ToInt32(reader["lancamento_id"]),
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                VendaId = BancoHelper.InteiroOuNulo(reader, "venda_id"),
                OrdemServicoId = BancoHelper.InteiroOuNulo(reader, "ordem_servico_id"),
                Tipo = reader["tipo"].ToString(),
                Categoria = reader["categoria"].ToString(),
                Descricao = reader["descricao"].ToString(),
                Valor = Convert.ToDecimal(reader["valor"]),
                FormaPagamento = reader["forma_pagamento"].ToString(),
                DataLancamento = Convert.ToDateTime(reader["data_lancamento"])
            };
        }
    }
}
