using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class OpcionalDAO : IDAO<Opcional>
    {
        public int Insert(Opcional opcional)
        {
            string sql = @"INSERT INTO opcionais (nome, descricao, preco_tabela, categoria)
                           VALUES (@nome, @descricao, @preco_tabela, @categoria)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", opcional.Nome);
                cmd.Parameters.AddWithValue("@descricao", BancoHelper.ParaBanco(opcional.Descricao));
                cmd.Parameters.AddWithValue("@preco_tabela", opcional.PrecoTabela);
                cmd.Parameters.AddWithValue("@categoria", BancoHelper.ParaBanco(opcional.Categoria));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Opcional opcional)
        {
            string sql = @"UPDATE opcionais SET
                    nome = @nome,
                    descricao = @descricao,
                    preco_tabela = @preco_tabela,
                    categoria = @categoria
                    WHERE opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", opcional.Nome);
                cmd.Parameters.AddWithValue("@descricao", BancoHelper.ParaBanco(opcional.Descricao));
                cmd.Parameters.AddWithValue("@preco_tabela", opcional.PrecoTabela);
                cmd.Parameters.AddWithValue("@categoria", BancoHelper.ParaBanco(opcional.Categoria));
                cmd.Parameters.AddWithValue("@opcional_id", opcional.OpcionalId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum opcional encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM opcionais WHERE opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@opcional_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum opcional encontrado com esse ID.");
            }
        }

        public List<Opcional> ListarTodos()
        {
            string sql = "SELECT * FROM opcionais";
            List<Opcional> opcionais = new List<Opcional>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    opcionais.Add(LerLinha(reader));
            }
            return opcionais;
        }

        public Opcional BuscarPorId(int id)
        {
            string sql = "SELECT * FROM opcionais WHERE opcional_id = @opcional_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@opcional_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Opcional
        private Opcional LerLinha(MySqlDataReader reader)
        {
            return new Opcional
            {
                OpcionalId = Convert.ToInt32(reader["opcional_id"]),
                Nome = reader["nome"].ToString(),
                Descricao = BancoHelper.TextoOuNulo(reader, "descricao"),
                PrecoTabela = Convert.ToDecimal(reader["preco_tabela"]),
                Categoria = BancoHelper.TextoOuNulo(reader, "categoria")
            };
        }
    }
}
