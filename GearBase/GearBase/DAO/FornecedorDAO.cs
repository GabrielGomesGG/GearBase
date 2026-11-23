using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class FornecedorDAO : IDAO<Fornecedor>
    {
        public int Insert(Fornecedor fornecedor)
        {
            string sql = @"INSERT INTO fornecedores (razao_social, nome_fantasia, cnpj, inscricao_estadual, email, telefone, contato_principal, endereco)
                           VALUES (@razao_social, @nome_fantasia, @cnpj, @inscricao_estadual, @email, @telefone, @contato_principal, @endereco)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@razao_social", fornecedor.RazaoSocial);
                cmd.Parameters.AddWithValue("@nome_fantasia", BancoHelper.ParaBanco(fornecedor.NomeFantasia));
                cmd.Parameters.AddWithValue("@cnpj", fornecedor.Cnpj);
                cmd.Parameters.AddWithValue("@inscricao_estadual", BancoHelper.ParaBanco(fornecedor.InscricaoEstadual));
                cmd.Parameters.AddWithValue("@email", fornecedor.Email);
                cmd.Parameters.AddWithValue("@telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@contato_principal", BancoHelper.ParaBanco(fornecedor.ContatoPrincipal));
                cmd.Parameters.AddWithValue("@endereco", BancoHelper.ParaBanco(fornecedor.Endereco));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Fornecedor fornecedor)
        {
            string sql = @"UPDATE fornecedores SET
                    razao_social = @razao_social,
                    nome_fantasia = @nome_fantasia,
                    cnpj = @cnpj,
                    inscricao_estadual = @inscricao_estadual,
                    email = @email,
                    telefone = @telefone,
                    contato_principal = @contato_principal,
                    endereco = @endereco
                    WHERE fornecedor_id = @fornecedor_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@razao_social", fornecedor.RazaoSocial);
                cmd.Parameters.AddWithValue("@nome_fantasia", BancoHelper.ParaBanco(fornecedor.NomeFantasia));
                cmd.Parameters.AddWithValue("@cnpj", fornecedor.Cnpj);
                cmd.Parameters.AddWithValue("@inscricao_estadual", BancoHelper.ParaBanco(fornecedor.InscricaoEstadual));
                cmd.Parameters.AddWithValue("@email", fornecedor.Email);
                cmd.Parameters.AddWithValue("@telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@contato_principal", BancoHelper.ParaBanco(fornecedor.ContatoPrincipal));
                cmd.Parameters.AddWithValue("@endereco", BancoHelper.ParaBanco(fornecedor.Endereco));
                cmd.Parameters.AddWithValue("@fornecedor_id", fornecedor.FornecedorId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum fornecedor encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM fornecedores WHERE fornecedor_id = @fornecedor_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@fornecedor_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum fornecedor encontrado com esse ID.");
            }
        }

        public List<Fornecedor> ListarTodos()
        {
            string sql = "SELECT * FROM fornecedores";
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    fornecedores.Add(LerLinha(reader));
            }
            return fornecedores;
        }

        public Fornecedor BuscarPorId(int id)
        {
            string sql = "SELECT * FROM fornecedores WHERE fornecedor_id = @fornecedor_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@fornecedor_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Fornecedor
        private Fornecedor LerLinha(MySqlDataReader reader)
        {
            return new Fornecedor
            {
                FornecedorId = Convert.ToInt32(reader["fornecedor_id"]),
                RazaoSocial = reader["razao_social"].ToString(),
                NomeFantasia = BancoHelper.TextoOuNulo(reader, "nome_fantasia"),
                Cnpj = reader["cnpj"].ToString(),
                InscricaoEstadual = BancoHelper.TextoOuNulo(reader, "inscricao_estadual"),
                Email = reader["email"].ToString(),
                Telefone = reader["telefone"].ToString(),
                ContatoPrincipal = BancoHelper.TextoOuNulo(reader, "contato_principal"),
                Endereco = BancoHelper.TextoOuNulo(reader, "endereco")
            };
        }
    }
}
