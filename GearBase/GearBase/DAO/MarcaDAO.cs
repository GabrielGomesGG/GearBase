using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class MarcaDAO : IDAO<Marca>
    {
        public int Insert(Marca marca)
        {
            string sql = @"INSERT INTO marcas (nome, pais_origem, site_oficial)
                           VALUES (@nome, @pais_origem, @site_oficial)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", marca.Nome);
                cmd.Parameters.AddWithValue("@pais_origem", BancoHelper.ParaBanco(marca.PaisOrigem));
                cmd.Parameters.AddWithValue("@site_oficial", BancoHelper.ParaBanco(marca.SiteOficial));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Marca marca)
        {
            string sql = @"UPDATE marcas SET
                    nome = @nome,
                    pais_origem = @pais_origem,
                    site_oficial = @site_oficial
                    WHERE marca_id = @marca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", marca.Nome);
                cmd.Parameters.AddWithValue("@pais_origem", BancoHelper.ParaBanco(marca.PaisOrigem));
                cmd.Parameters.AddWithValue("@site_oficial", BancoHelper.ParaBanco(marca.SiteOficial));
                cmd.Parameters.AddWithValue("@marca_id", marca.MarcaId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma marca encontrada com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM marcas WHERE marca_id = @marca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@marca_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma marca encontrada com esse ID.");
            }
        }

        public List<Marca> ListarTodos()
        {
            string sql = "SELECT * FROM marcas";
            List<Marca> marcas = new List<Marca>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    marcas.Add(LerLinha(reader));
            }
            return marcas;
        }

        public Marca BuscarPorId(int id)
        {
            string sql = "SELECT * FROM marcas WHERE marca_id = @marca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@marca_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Marca
        private Marca LerLinha(MySqlDataReader reader)
        {
            return new Marca
            {
                MarcaId = Convert.ToInt32(reader["marca_id"]),
                Nome = reader["nome"].ToString(),
                PaisOrigem = BancoHelper.TextoOuNulo(reader, "pais_origem"),
                SiteOficial = BancoHelper.TextoOuNulo(reader, "site_oficial")
            };
        }
    }
}
