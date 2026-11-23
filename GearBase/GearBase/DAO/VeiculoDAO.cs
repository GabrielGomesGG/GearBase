using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class VeiculoDAO : IDAO<Veiculo>
    {
        public int Insert(Veiculo veiculo)
        {
            ConferirAnos(veiculo);

            string sql = @"INSERT INTO veiculos (modelo_id, cliente_id, chassi, placa, ano_fabricacao, ano_modelo, cor, combustivel, transmissao, motorizacao, potencia_cv, portas, quilometragem, preco_custo, preco_venda, tipo, status, data_entrada)
                           VALUES (@modelo_id, @cliente_id, @chassi, @placa, @ano_fabricacao, @ano_modelo, @cor, @combustivel, @transmissao, @motorizacao, @potencia_cv, @portas, @quilometragem, @preco_custo, @preco_venda, @tipo, @status, @data_entrada)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@modelo_id", veiculo.ModeloId);
                cmd.Parameters.AddWithValue("@cliente_id", BancoHelper.ParaBanco(veiculo.ClienteId));
                cmd.Parameters.AddWithValue("@chassi", veiculo.Chassi);
                cmd.Parameters.AddWithValue("@placa", BancoHelper.ParaBanco(veiculo.Placa));
                cmd.Parameters.AddWithValue("@ano_fabricacao", veiculo.AnoFabricacao);
                cmd.Parameters.AddWithValue("@ano_modelo", veiculo.AnoModelo);
                cmd.Parameters.AddWithValue("@cor", veiculo.Cor);
                cmd.Parameters.AddWithValue("@combustivel", veiculo.Combustivel);
                cmd.Parameters.AddWithValue("@transmissao", veiculo.Transmissao);
                cmd.Parameters.AddWithValue("@motorizacao", veiculo.Motorizacao);
                cmd.Parameters.AddWithValue("@potencia_cv", BancoHelper.ParaBanco(veiculo.PotenciaCv));
                cmd.Parameters.AddWithValue("@portas", veiculo.Portas);
                cmd.Parameters.AddWithValue("@quilometragem", veiculo.Quilometragem);
                cmd.Parameters.AddWithValue("@preco_custo", veiculo.PrecoCusto);
                cmd.Parameters.AddWithValue("@preco_venda", veiculo.PrecoVenda);
                cmd.Parameters.AddWithValue("@tipo", veiculo.Tipo);
                cmd.Parameters.AddWithValue("@status", veiculo.Status);
                cmd.Parameters.AddWithValue("@data_entrada", BancoHelper.ParaBanco(veiculo.DataEntrada));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Veiculo veiculo)
        {
            ConferirAnos(veiculo);

            string sql = @"UPDATE veiculos SET
                    modelo_id = @modelo_id,
                    cliente_id = @cliente_id,
                    chassi = @chassi,
                    placa = @placa,
                    ano_fabricacao = @ano_fabricacao,
                    ano_modelo = @ano_modelo,
                    cor = @cor,
                    combustivel = @combustivel,
                    transmissao = @transmissao,
                    motorizacao = @motorizacao,
                    potencia_cv = @potencia_cv,
                    portas = @portas,
                    quilometragem = @quilometragem,
                    preco_custo = @preco_custo,
                    preco_venda = @preco_venda,
                    tipo = @tipo,
                    status = @status,
                    data_entrada = @data_entrada
                    WHERE veiculo_id = @veiculo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@modelo_id", veiculo.ModeloId);
                cmd.Parameters.AddWithValue("@cliente_id", BancoHelper.ParaBanco(veiculo.ClienteId));
                cmd.Parameters.AddWithValue("@chassi", veiculo.Chassi);
                cmd.Parameters.AddWithValue("@placa", BancoHelper.ParaBanco(veiculo.Placa));
                cmd.Parameters.AddWithValue("@ano_fabricacao", veiculo.AnoFabricacao);
                cmd.Parameters.AddWithValue("@ano_modelo", veiculo.AnoModelo);
                cmd.Parameters.AddWithValue("@cor", veiculo.Cor);
                cmd.Parameters.AddWithValue("@combustivel", veiculo.Combustivel);
                cmd.Parameters.AddWithValue("@transmissao", veiculo.Transmissao);
                cmd.Parameters.AddWithValue("@motorizacao", veiculo.Motorizacao);
                cmd.Parameters.AddWithValue("@potencia_cv", BancoHelper.ParaBanco(veiculo.PotenciaCv));
                cmd.Parameters.AddWithValue("@portas", veiculo.Portas);
                cmd.Parameters.AddWithValue("@quilometragem", veiculo.Quilometragem);
                cmd.Parameters.AddWithValue("@preco_custo", veiculo.PrecoCusto);
                cmd.Parameters.AddWithValue("@preco_venda", veiculo.PrecoVenda);
                cmd.Parameters.AddWithValue("@tipo", veiculo.Tipo);
                cmd.Parameters.AddWithValue("@status", veiculo.Status);
                cmd.Parameters.AddWithValue("@data_entrada", BancoHelper.ParaBanco(veiculo.DataEntrada));
                cmd.Parameters.AddWithValue("@veiculo_id", veiculo.VeiculoId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum veículo encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            // Veículo com venda, test drive ou OS não pode sair: o histórico depende dele
            string sqlUso = @"SELECT (SELECT COUNT(*) FROM vendas WHERE veiculo_id = @veiculo_id)
                                   + (SELECT COUNT(*) FROM test_drives WHERE veiculo_id = @veiculo_id)
                                   + (SELECT COUNT(*) FROM ordens_servico WHERE veiculo_id = @veiculo_id) AS total";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sqlUso, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && Convert.ToInt32(reader["total"]) > 0)
                        throw new Exception("Este veículo tem vendas, test drives ou ordens de serviço e não pode ser excluído.");
                }
            }

            // Os opcionais do veículo saem antes, senão a chave estrangeira não deixa apagar o veículo
            new VeiculoOpcionalDAO().DeletarDoVeiculo(id);

            string sql = "DELETE FROM veiculos WHERE veiculo_id = @veiculo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum veículo encontrado com esse ID.");
            }
        }

        public List<Veiculo> ListarTodos()
        {
            string sql = "SELECT * FROM veiculos";
            List<Veiculo> veiculos = new List<Veiculo>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    veiculos.Add(LerLinha(reader));
            }
            return veiculos;
        }

        public Veiculo BuscarPorId(int id)
        {
            string sql = "SELECT * FROM veiculos WHERE veiculo_id = @veiculo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Usado nas regras de venda, test drive e ordem de serviço: o veículo escolhido precisa existir
        public Veiculo BuscarObrigatorio(int veiculoId)
        {
            Veiculo veiculo = BuscarPorId(veiculoId);
            if (veiculo == null)
                throw new Exception("Veículo não encontrado.");
            return veiculo;
        }

        // Passa o veículo para um cliente (venda concluída) ou deixa com a concessionária (clienteId null)
        public void TrocarDono(int veiculoId, int? clienteId, string status)
        {
            string sql = "UPDATE veiculos SET cliente_id = @cliente_id, status = @status WHERE veiculo_id = @veiculo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", BancoHelper.ParaBanco(clienteId));
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoId);
                cmd.ExecuteNonQuery();
            }
        }

        /* Venda cancelada ou excluída: o veículo volta para o estoque, mas só se ainda estava com esse cliente
           (ou reservado para ele), para não desfazer uma venda mais nova do mesmo veículo */
        public void DevolverAoEstoque(int veiculoId, int clienteId)
        {
            Veiculo veiculo = BuscarPorId(veiculoId);
            if (veiculo == null)
                return;

            bool comEsseCliente = veiculo.ClienteId == clienteId;
            bool reservado = veiculo.ClienteId == null && veiculo.Status == "Reservado";

            if (comEsseCliente || reservado)
                TrocarDono(veiculoId, null, "Disponível");
        }

        private void ConferirAnos(Veiculo veiculo)
        {
            if (veiculo.AnoModelo < veiculo.AnoFabricacao)
                throw new Exception("O ano do modelo não pode ser menor que o ano de fabricação.");
        }

        // Converte uma linha do banco em um objeto Veiculo
        private Veiculo LerLinha(MySqlDataReader reader)
        {
            return new Veiculo
            {
                VeiculoId = Convert.ToInt32(reader["veiculo_id"]),
                ModeloId = Convert.ToInt32(reader["modelo_id"]),
                ClienteId = BancoHelper.InteiroOuNulo(reader, "cliente_id"),
                Chassi = reader["chassi"].ToString(),
                Placa = BancoHelper.TextoOuNulo(reader, "placa"),
                AnoFabricacao = Convert.ToInt32(reader["ano_fabricacao"]),
                AnoModelo = Convert.ToInt32(reader["ano_modelo"]),
                Cor = reader["cor"].ToString(),
                Combustivel = reader["combustivel"].ToString(),
                Transmissao = reader["transmissao"].ToString(),
                Motorizacao = reader["motorizacao"].ToString(),
                PotenciaCv = BancoHelper.InteiroOuNulo(reader, "potencia_cv"),
                Portas = Convert.ToInt32(reader["portas"]),
                Quilometragem = Convert.ToInt32(reader["quilometragem"]),
                PrecoCusto = Convert.ToDecimal(reader["preco_custo"]),
                PrecoVenda = Convert.ToDecimal(reader["preco_venda"]),
                Tipo = reader["tipo"].ToString(),
                Status = reader["status"].ToString(),
                DataEntrada = BancoHelper.LerData(reader, "data_entrada")
            };
        }
    }
}
