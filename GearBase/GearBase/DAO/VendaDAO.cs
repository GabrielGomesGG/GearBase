using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class VendaDAO : IDAO<Venda>
    {
        public int Insert(Venda venda)
        {
            venda.CalcularValorTotal();
            new FuncionarioDAO().ConferirPermissao(venda.FuncionarioId, "pode_vender", "registrar vendas", true);

            // Só se vende o que está no estoque da loja
            Veiculo veiculo = new VeiculoDAO().BuscarObrigatorio(venda.VeiculoId);
            if (veiculo.ClienteId != null || veiculo.Status != "Disponível")
                throw new Exception("Este veículo não está disponível no estoque da concessionária.");

            string sql = @"INSERT INTO vendas (cliente_id, funcionario_id, veiculo_id, data_venda, valor_veiculo, valor_entrada, desconto, valor_total, forma_pagamento, quantidade_parcelas, taxa_juros, status_venda, observacoes)
                           VALUES (@cliente_id, @funcionario_id, @veiculo_id, @data_venda, @valor_veiculo, @valor_entrada, @desconto, @valor_total, @forma_pagamento, @quantidade_parcelas, @taxa_juros, @status_venda, @observacoes)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", venda.ClienteId);
                cmd.Parameters.AddWithValue("@funcionario_id", venda.FuncionarioId);
                cmd.Parameters.AddWithValue("@veiculo_id", venda.VeiculoId);
                cmd.Parameters.AddWithValue("@data_venda", venda.DataVenda);
                cmd.Parameters.AddWithValue("@valor_veiculo", venda.ValorVeiculo);
                cmd.Parameters.AddWithValue("@valor_entrada", BancoHelper.ParaBanco(venda.ValorEntrada));
                cmd.Parameters.AddWithValue("@desconto", BancoHelper.ParaBanco(venda.Desconto));
                cmd.Parameters.AddWithValue("@valor_total", venda.ValorTotal);
                cmd.Parameters.AddWithValue("@forma_pagamento", venda.FormaPagamento);
                cmd.Parameters.AddWithValue("@quantidade_parcelas", BancoHelper.ParaBanco(venda.QuantidadeParcelas));
                cmd.Parameters.AddWithValue("@taxa_juros", BancoHelper.ParaBanco(venda.TaxaJuros));
                cmd.Parameters.AddWithValue("@status_venda", venda.StatusVenda);
                cmd.Parameters.AddWithValue("@observacoes", BancoHelper.ParaBanco(venda.Observacoes));
                cmd.ExecuteNonQuery();
                venda.VendaId = (int)cmd.LastInsertedId;
            }

            AtualizarVeiculo(venda);
            new LancamentoCaixaDAO().SincronizarVenda(venda.VendaId);
            return venda.VendaId;
        }

        public void Update(Venda venda)
        {
            venda.CalcularValorTotal();
            new FuncionarioDAO().ConferirPermissao(venda.FuncionarioId, "pode_vender", "registrar vendas", false);

            Venda anterior = BuscarPorId(venda.VendaId);
            if (anterior == null)
                throw new Exception("Nenhuma venda encontrada com esse ID.");

            Veiculo veiculo = new VeiculoDAO().BuscarObrigatorio(venda.VeiculoId);
            // O veículo pode estar no estoque, já ser do cliente da venda ou ter ido para o cliente por esta mesma venda
            bool desta = anterior.VeiculoId == venda.VeiculoId && veiculo.ClienteId == anterior.ClienteId;
            if (veiculo.ClienteId != null && veiculo.ClienteId != venda.ClienteId && !desta)
                throw new Exception("Este veículo pertence a outro cliente.");

            string sql = @"UPDATE vendas SET
                    cliente_id = @cliente_id,
                    funcionario_id = @funcionario_id,
                    veiculo_id = @veiculo_id,
                    data_venda = @data_venda,
                    valor_veiculo = @valor_veiculo,
                    valor_entrada = @valor_entrada,
                    desconto = @desconto,
                    valor_total = @valor_total,
                    forma_pagamento = @forma_pagamento,
                    quantidade_parcelas = @quantidade_parcelas,
                    taxa_juros = @taxa_juros,
                    status_venda = @status_venda,
                    observacoes = @observacoes
                    WHERE venda_id = @venda_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", venda.ClienteId);
                cmd.Parameters.AddWithValue("@funcionario_id", venda.FuncionarioId);
                cmd.Parameters.AddWithValue("@veiculo_id", venda.VeiculoId);
                cmd.Parameters.AddWithValue("@data_venda", venda.DataVenda);
                cmd.Parameters.AddWithValue("@valor_veiculo", venda.ValorVeiculo);
                cmd.Parameters.AddWithValue("@valor_entrada", BancoHelper.ParaBanco(venda.ValorEntrada));
                cmd.Parameters.AddWithValue("@desconto", BancoHelper.ParaBanco(venda.Desconto));
                cmd.Parameters.AddWithValue("@valor_total", venda.ValorTotal);
                cmd.Parameters.AddWithValue("@forma_pagamento", venda.FormaPagamento);
                cmd.Parameters.AddWithValue("@quantidade_parcelas", BancoHelper.ParaBanco(venda.QuantidadeParcelas));
                cmd.Parameters.AddWithValue("@taxa_juros", BancoHelper.ParaBanco(venda.TaxaJuros));
                cmd.Parameters.AddWithValue("@status_venda", venda.StatusVenda);
                cmd.Parameters.AddWithValue("@observacoes", BancoHelper.ParaBanco(venda.Observacoes));
                cmd.Parameters.AddWithValue("@venda_id", venda.VendaId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma venda encontrada com esse ID.");
            }

            // Trocou o veículo da venda: o antigo volta para o estoque
            if (anterior.VeiculoId != venda.VeiculoId && anterior.StatusVenda != "Cancelada")
                new VeiculoDAO().DevolverAoEstoque(anterior.VeiculoId, anterior.ClienteId);
            AtualizarVeiculo(venda);
            new LancamentoCaixaDAO().SincronizarVenda(venda.VendaId);
        }

        public void Delete(int id)
        {
            Venda anterior = BuscarPorId(id);

            // O lançamento do caixa aponta para a venda, então sai antes dela
            new LancamentoCaixaDAO().ApagarDaVenda(id);

            string sql = "DELETE FROM vendas WHERE venda_id = @venda_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@venda_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma venda encontrada com esse ID.");
            }

            if (anterior != null && anterior.StatusVenda != "Cancelada")
                new VeiculoDAO().DevolverAoEstoque(anterior.VeiculoId, anterior.ClienteId);
        }

        /* A situação da venda decide de quem é o veículo:
           concluída passa para o cliente, pendente fica reservado e cancelada devolve ao estoque */
        private void AtualizarVeiculo(Venda venda)
        {
            var veiculoDAO = new VeiculoDAO();
            if (venda.StatusVenda == "Concluída")
                veiculoDAO.TrocarDono(venda.VeiculoId, venda.ClienteId, "Vendido");
            else if (venda.StatusVenda == "Pendente")
                veiculoDAO.TrocarDono(venda.VeiculoId, null, "Reservado");
            else
                veiculoDAO.DevolverAoEstoque(venda.VeiculoId, venda.ClienteId);
        }

        public List<Venda> ListarTodos()
        {
            string sql = "SELECT * FROM vendas";
            List<Venda> vendas = new List<Venda>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    vendas.Add(LerLinha(reader));
            }
            return vendas;
        }

        public Venda BuscarPorId(int id)
        {
            string sql = "SELECT * FROM vendas WHERE venda_id = @venda_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@venda_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Venda
        private Venda LerLinha(MySqlDataReader reader)
        {
            return new Venda
            {
                VendaId = Convert.ToInt32(reader["venda_id"]),
                ClienteId = Convert.ToInt32(reader["cliente_id"]),
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                VeiculoId = Convert.ToInt32(reader["veiculo_id"]),
                DataVenda = Convert.ToDateTime(reader["data_venda"]),
                ValorVeiculo = Convert.ToDecimal(reader["valor_veiculo"]),
                ValorEntrada = BancoHelper.DecimalOuNulo(reader, "valor_entrada"),
                Desconto = BancoHelper.DecimalOuNulo(reader, "desconto"),
                ValorTotal = Convert.ToDecimal(reader["valor_total"]),
                FormaPagamento = reader["forma_pagamento"].ToString(),
                QuantidadeParcelas = BancoHelper.InteiroOuNulo(reader, "quantidade_parcelas"),
                TaxaJuros = BancoHelper.DecimalOuNulo(reader, "taxa_juros"),
                StatusVenda = reader["status_venda"].ToString(),
                Observacoes = BancoHelper.TextoOuNulo(reader, "observacoes")
            };
        }
    }
}
