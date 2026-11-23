using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class ModeloDAO : IDAO<Modelo>
    {
        public int Insert(Modelo modelo)
        {
            string sql = @"INSERT INTO modelos (marca_id, nome, categoria, ano_lancamento)
                           VALUES (@marca_id, @nome, @categoria, @ano_lancamento)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@marca_id", modelo.MarcaId);
                cmd.Parameters.AddWithValue("@nome", modelo.Nome);
                cmd.Parameters.AddWithValue("@categoria", modelo.Categoria);
                cmd.Parameters.AddWithValue("@ano_lancamento", BancoHelper.ParaBanco(modelo.AnoLancamento));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Modelo modelo)
        {
            string sql = @"UPDATE modelos SET
                    marca_id = @marca_id,
                    nome = @nome,
                    categoria = @categoria,
                    ano_lancamento = @ano_lancamento
                    WHERE modelo_id = @modelo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@marca_id", modelo.MarcaId);
                cmd.Parameters.AddWithValue("@nome", modelo.Nome);
                cmd.Parameters.AddWithValue("@categoria", modelo.Categoria);
                cmd.Parameters.AddWithValue("@ano_lancamento", BancoHelper.ParaBanco(modelo.AnoLancamento));
                cmd.Parameters.AddWithValue("@modelo_id", modelo.ModeloId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum modelo encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM modelos WHERE modelo_id = @modelo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@modelo_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum modelo encontrado com esse ID.");
            }
        }

        public List<Modelo> ListarTodos()
        {
            string sql = "SELECT * FROM modelos";
            List<Modelo> modelos = new List<Modelo>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    modelos.Add(LerLinha(reader));
            }
            return modelos;
        }

        public Modelo BuscarPorId(int id)
        {
            string sql = "SELECT * FROM modelos WHERE modelo_id = @modelo_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@modelo_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Modelo
        private Modelo LerLinha(MySqlDataReader reader)
        {
            return new Modelo
            {
                ModeloId = Convert.ToInt32(reader["modelo_id"]),
                MarcaId = Convert.ToInt32(reader["marca_id"]),
                Nome = reader["nome"].ToString(),
                Categoria = reader["categoria"].ToString(),
                AnoLancamento = BancoHelper.InteiroOuNulo(reader, "ano_lancamento")
            };
        }
    }
}
