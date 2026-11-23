using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class TestDriveDAO : IDAO<TestDrive>
    {
        public int Insert(TestDrive testDrive)
        {
            ConferirRegras(testDrive, true);

            string sql = @"INSERT INTO test_drives (cliente_id, veiculo_id, funcionario_id, data_agendamento, cnh_cliente, validade_cnh, km_inicial, km_final, status, avaliacao_cliente, observacoes)
                           VALUES (@cliente_id, @veiculo_id, @funcionario_id, @data_agendamento, @cnh_cliente, @validade_cnh, @km_inicial, @km_final, @status, @avaliacao_cliente, @observacoes)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", testDrive.ClienteId);
                cmd.Parameters.AddWithValue("@veiculo_id", testDrive.VeiculoId);
                cmd.Parameters.AddWithValue("@funcionario_id", testDrive.FuncionarioId);
                cmd.Parameters.AddWithValue("@data_agendamento", testDrive.DataAgendamento);
                cmd.Parameters.AddWithValue("@cnh_cliente", testDrive.CnhCliente);
                cmd.Parameters.AddWithValue("@validade_cnh", BancoHelper.ParaBanco(testDrive.ValidadeCnh));
                cmd.Parameters.AddWithValue("@km_inicial", BancoHelper.ParaBanco(testDrive.KmInicial));
                cmd.Parameters.AddWithValue("@km_final", BancoHelper.ParaBanco(testDrive.KmFinal));
                cmd.Parameters.AddWithValue("@status", testDrive.Status);
                cmd.Parameters.AddWithValue("@avaliacao_cliente", BancoHelper.ParaBanco(testDrive.AvaliacaoCliente));
                cmd.Parameters.AddWithValue("@observacoes", BancoHelper.ParaBanco(testDrive.Observacoes));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(TestDrive testDrive)
        {
            ConferirRegras(testDrive, false);

            string sql = @"UPDATE test_drives SET
                    cliente_id = @cliente_id,
                    veiculo_id = @veiculo_id,
                    funcionario_id = @funcionario_id,
                    data_agendamento = @data_agendamento,
                    cnh_cliente = @cnh_cliente,
                    validade_cnh = @validade_cnh,
                    km_inicial = @km_inicial,
                    km_final = @km_final,
                    status = @status,
                    avaliacao_cliente = @avaliacao_cliente,
                    observacoes = @observacoes
                    WHERE test_drive_id = @test_drive_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", testDrive.ClienteId);
                cmd.Parameters.AddWithValue("@veiculo_id", testDrive.VeiculoId);
                cmd.Parameters.AddWithValue("@funcionario_id", testDrive.FuncionarioId);
                cmd.Parameters.AddWithValue("@data_agendamento", testDrive.DataAgendamento);
                cmd.Parameters.AddWithValue("@cnh_cliente", testDrive.CnhCliente);
                cmd.Parameters.AddWithValue("@validade_cnh", BancoHelper.ParaBanco(testDrive.ValidadeCnh));
                cmd.Parameters.AddWithValue("@km_inicial", BancoHelper.ParaBanco(testDrive.KmInicial));
                cmd.Parameters.AddWithValue("@km_final", BancoHelper.ParaBanco(testDrive.KmFinal));
                cmd.Parameters.AddWithValue("@status", testDrive.Status);
                cmd.Parameters.AddWithValue("@avaliacao_cliente", BancoHelper.ParaBanco(testDrive.AvaliacaoCliente));
                cmd.Parameters.AddWithValue("@observacoes", BancoHelper.ParaBanco(testDrive.Observacoes));
                cmd.Parameters.AddWithValue("@test_drive_id", testDrive.TestDriveId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum test drive encontrado com esse ID.");
            }
        }

        // Quem acompanha precisa poder vender, e test drive em aberto só com carro disponível da loja
        private void ConferirRegras(TestDrive testDrive, bool cadastroNovo)
        {
            new FuncionarioDAO().ConferirPermissao(testDrive.FuncionarioId, "pode_vender", "acompanhar test drives", cadastroNovo);

            if (testDrive.KmInicial != null && testDrive.KmFinal != null && testDrive.KmFinal < testDrive.KmInicial)
                throw new Exception("A quilometragem final não pode ser menor que a inicial.");

            if (testDrive.Status == "Agendado" || testDrive.Status == "Em Andamento")
            {
                Veiculo veiculo = new VeiculoDAO().BuscarObrigatorio(testDrive.VeiculoId);
                if (veiculo.ClienteId != null || veiculo.Status != "Disponível")
                    throw new Exception("Test drive em aberto só pode usar veículo disponível no estoque da concessionária.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM test_drives WHERE test_drive_id = @test_drive_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@test_drive_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum test drive encontrado com esse ID.");
            }
        }

        public List<TestDrive> ListarTodos()
        {
            string sql = "SELECT * FROM test_drives";
            List<TestDrive> testDrives = new List<TestDrive>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    testDrives.Add(LerLinha(reader));
            }
            return testDrives;
        }

        public TestDrive BuscarPorId(int id)
        {
            string sql = "SELECT * FROM test_drives WHERE test_drive_id = @test_drive_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@test_drive_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto TestDrive
        private TestDrive LerLinha(MySqlDataReader reader)
        {
            return new TestDrive
            {
                TestDriveId = Convert.ToInt32(reader["test_drive_id"]),
                ClienteId = Convert.ToInt32(reader["cliente_id"]),
                VeiculoId = Convert.ToInt32(reader["veiculo_id"]),
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                DataAgendamento = Convert.ToDateTime(reader["data_agendamento"]),
                CnhCliente = reader["cnh_cliente"].ToString(),
                ValidadeCnh = BancoHelper.LerData(reader, "validade_cnh"),
                KmInicial = BancoHelper.InteiroOuNulo(reader, "km_inicial"),
                KmFinal = BancoHelper.InteiroOuNulo(reader, "km_final"),
                Status = reader["status"].ToString(),
                AvaliacaoCliente = BancoHelper.InteiroOuNulo(reader, "avaliacao_cliente"),
                Observacoes = BancoHelper.TextoOuNulo(reader, "observacoes")
            };
        }
    }
}
