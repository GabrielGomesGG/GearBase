using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class CargoDAO : IDAO<Cargo>
    {
        public int Insert(Cargo cargo)
        {
            string sql = @"INSERT INTO cargos (nome, salario_base, comissao_percentual, pode_vender, pode_abrir_os, pode_liberar, pode_financeiro)
                           VALUES (@nome, @salario_base, @comissao_percentual, @pode_vender, @pode_abrir_os, @pode_liberar, @pode_financeiro)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cargo.Nome);
                cmd.Parameters.AddWithValue("@salario_base", cargo.SalarioBase);
                cmd.Parameters.AddWithValue("@comissao_percentual", cargo.ComissaoPercentual);
                cmd.Parameters.AddWithValue("@pode_vender", cargo.PodeVender);
                cmd.Parameters.AddWithValue("@pode_abrir_os", cargo.PodeAbrirOs);
                cmd.Parameters.AddWithValue("@pode_liberar", cargo.PodeLiberar);
                cmd.Parameters.AddWithValue("@pode_financeiro", cargo.PodeFinanceiro);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Cargo cargo)
        {
            string sql = @"UPDATE cargos SET
                    nome = @nome,
                    salario_base = @salario_base,
                    comissao_percentual = @comissao_percentual,
                    pode_vender = @pode_vender,
                    pode_abrir_os = @pode_abrir_os,
                    pode_liberar = @pode_liberar,
                    pode_financeiro = @pode_financeiro
                    WHERE cargo_id = @cargo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cargo.Nome);
                cmd.Parameters.AddWithValue("@salario_base", cargo.SalarioBase);
                cmd.Parameters.AddWithValue("@comissao_percentual", cargo.ComissaoPercentual);
                cmd.Parameters.AddWithValue("@pode_vender", cargo.PodeVender);
                cmd.Parameters.AddWithValue("@pode_abrir_os", cargo.PodeAbrirOs);
                cmd.Parameters.AddWithValue("@pode_liberar", cargo.PodeLiberar);
                cmd.Parameters.AddWithValue("@pode_financeiro", cargo.PodeFinanceiro);
                cmd.Parameters.AddWithValue("@cargo_id", cargo.CargoId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum cargo encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM cargos WHERE cargo_id = @cargo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cargo_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum cargo encontrado com esse ID.");
            }
        }

        public List<Cargo> ListarTodos()
        {
            string sql = "SELECT * FROM cargos";
            List<Cargo> cargos = new List<Cargo>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    cargos.Add(LerLinha(reader));
            }
            return cargos;
        }

        public Cargo BuscarPorId(int id)
        {
            string sql = "SELECT * FROM cargos WHERE cargo_id = @cargo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cargo_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Cargo
        private Cargo LerLinha(MySqlDataReader reader)
        {
            return new Cargo
            {
                CargoId = Convert.ToInt32(reader["cargo_id"]),
                Nome = reader["nome"].ToString(),
                SalarioBase = Convert.ToDecimal(reader["salario_base"]),
                ComissaoPercentual = Convert.ToDecimal(reader["comissao_percentual"]),
                PodeVender = Convert.ToBoolean(reader["pode_vender"]),
                PodeAbrirOs = Convert.ToBoolean(reader["pode_abrir_os"]),
                PodeLiberar = Convert.ToBoolean(reader["pode_liberar"]),
                PodeFinanceiro = Convert.ToBoolean(reader["pode_financeiro"])
            };
        }
    }
}
