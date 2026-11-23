using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class VeiculoOpcionalDAO : IVeiculoOpcionalDAO
    {
        public void Insert(VeiculoOpcional veiculoOpcional)
        {
            string sql = @"INSERT INTO veiculo_opcionais (veiculo_id, opcional_id, preco_aplicado, instalado_fabrica)
                           VALUES (@veiculo_id, @opcional_id, @preco_aplicado, @instalado_fabrica)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoOpcional.VeiculoId);
                cmd.Parameters.AddWithValue("@opcional_id", veiculoOpcional.OpcionalId);
                cmd.Parameters.AddWithValue("@preco_aplicado", veiculoOpcional.PrecoAplicado);
                cmd.Parameters.AddWithValue("@instalado_fabrica", veiculoOpcional.InstaladoFabrica);
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(VeiculoOpcional veiculoOpcional)
        {
            string sql = @"UPDATE veiculo_opcionais SET
                    preco_aplicado = @preco_aplicado,
                    instalado_fabrica = @instalado_fabrica
                    WHERE veiculo_id = @veiculo_id AND opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@preco_aplicado", veiculoOpcional.PrecoAplicado);
                cmd.Parameters.AddWithValue("@instalado_fabrica", veiculoOpcional.InstaladoFabrica);
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoOpcional.VeiculoId);
                cmd.Parameters.AddWithValue("@opcional_id", veiculoOpcional.OpcionalId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Esse opcional não está vinculado a esse veículo.");
            }
        }

        public void Delete(int veiculoId, int opcionalId)
        {
            string sql = "DELETE FROM veiculo_opcionais WHERE veiculo_id = @veiculo_id AND opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoId);
                cmd.Parameters.AddWithValue("@opcional_id", opcionalId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Esse opcional não está vinculado a esse veículo.");
            }
        }

        // Apaga todos os opcionais de um veículo (usado antes de excluir o veículo)
        public void DeletarDoVeiculo(int veiculoId)
        {
            string sql = "DELETE FROM veiculo_opcionais WHERE veiculo_id = @veiculo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<VeiculoOpcional> ListarTodos()
        {
            string sql = "SELECT * FROM veiculo_opcionais";
            List<VeiculoOpcional> lista = new List<VeiculoOpcional>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(LerLinha(reader));
            }
            return lista;
        }

        public List<VeiculoOpcional> ListarPorVeiculo(int veiculoId)
        {
            string sql = "SELECT * FROM veiculo_opcionais WHERE veiculo_id = @veiculo_id";
            List<VeiculoOpcional> lista = new List<VeiculoOpcional>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(LerLinha(reader));
                }
            }
            return lista;
        }

        public VeiculoOpcional BuscarPorId(int veiculoId, int opcionalId)
        {
            string sql = "SELECT * FROM veiculo_opcionais WHERE veiculo_id = @veiculo_id AND opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@veiculo_id", veiculoId);
                cmd.Parameters.AddWithValue("@opcional_id", opcionalId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto VeiculoOpcional
        private VeiculoOpcional LerLinha(MySqlDataReader reader)
        {
            return new VeiculoOpcional
            {
                VeiculoId = Convert.ToInt32(reader["veiculo_id"]),
                OpcionalId = Convert.ToInt32(reader["opcional_id"]),
                PrecoAplicado = Convert.ToDecimal(reader["preco_aplicado"]),
                InstaladoFabrica = Convert.ToBoolean(reader["instalado_fabrica"])
            };
        }
    }
}
