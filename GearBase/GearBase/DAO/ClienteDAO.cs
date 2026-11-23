using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class ClienteDAO : IDAO<Cliente>
    {
        public int Insert(Cliente cliente)
        {
            string sql = @"INSERT INTO clientes (nome, tipo_pessoa, sexo, cpf_cnpj, email, telefone, data_nascimento, logradouro, numero, bairro, cidade, estado, cep, data_cadastro)
                           VALUES (@nome, @tipo_pessoa, @sexo, @cpf_cnpj, @email, @telefone, @data_nascimento, @logradouro, @numero, @bairro, @cidade, @estado, @cep, @data_cadastro)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cliente.Nome);
                cmd.Parameters.AddWithValue("@tipo_pessoa", cliente.TipoPessoa);
                cmd.Parameters.AddWithValue("@sexo", BancoHelper.ParaBanco(cliente.Sexo));
                cmd.Parameters.AddWithValue("@cpf_cnpj", cliente.CpfCnpj);
                cmd.Parameters.AddWithValue("@email", BancoHelper.ParaBanco(cliente.Email));
                cmd.Parameters.AddWithValue("@telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@data_nascimento", BancoHelper.ParaBanco(cliente.DataNascimento));
                cmd.Parameters.AddWithValue("@logradouro", BancoHelper.ParaBanco(cliente.Logradouro));
                cmd.Parameters.AddWithValue("@numero", BancoHelper.ParaBanco(cliente.Numero));
                cmd.Parameters.AddWithValue("@bairro", BancoHelper.ParaBanco(cliente.Bairro));
                cmd.Parameters.AddWithValue("@cidade", BancoHelper.ParaBanco(cliente.Cidade));
                cmd.Parameters.AddWithValue("@estado", BancoHelper.ParaBanco(cliente.Estado));
                cmd.Parameters.AddWithValue("@cep", BancoHelper.ParaBanco(cliente.Cep));
                cmd.Parameters.AddWithValue("@data_cadastro", DateTime.Now);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Cliente cliente)
        {
            string sql = @"UPDATE clientes SET
                    nome = @nome,
                    tipo_pessoa = @tipo_pessoa,
                    sexo = @sexo,
                    cpf_cnpj = @cpf_cnpj,
                    email = @email,
                    telefone = @telefone,
                    data_nascimento = @data_nascimento,
                    logradouro = @logradouro,
                    numero = @numero,
                    bairro = @bairro,
                    cidade = @cidade,
                    estado = @estado,
                    cep = @cep
                    WHERE cliente_id = @cliente_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", cliente.Nome);
                cmd.Parameters.AddWithValue("@tipo_pessoa", cliente.TipoPessoa);
                cmd.Parameters.AddWithValue("@sexo", BancoHelper.ParaBanco(cliente.Sexo));
                cmd.Parameters.AddWithValue("@cpf_cnpj", cliente.CpfCnpj);
                cmd.Parameters.AddWithValue("@email", BancoHelper.ParaBanco(cliente.Email));
                cmd.Parameters.AddWithValue("@telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@data_nascimento", BancoHelper.ParaBanco(cliente.DataNascimento));
                cmd.Parameters.AddWithValue("@logradouro", BancoHelper.ParaBanco(cliente.Logradouro));
                cmd.Parameters.AddWithValue("@numero", BancoHelper.ParaBanco(cliente.Numero));
                cmd.Parameters.AddWithValue("@bairro", BancoHelper.ParaBanco(cliente.Bairro));
                cmd.Parameters.AddWithValue("@cidade", BancoHelper.ParaBanco(cliente.Cidade));
                cmd.Parameters.AddWithValue("@estado", BancoHelper.ParaBanco(cliente.Estado));
                cmd.Parameters.AddWithValue("@cep", BancoHelper.ParaBanco(cliente.Cep));
                cmd.Parameters.AddWithValue("@cliente_id", cliente.ClienteId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum cliente encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM clientes WHERE cliente_id = @cliente_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum cliente encontrado com esse ID.");
            }
        }

        public List<Cliente> ListarTodos()
        {
            string sql = "SELECT * FROM clientes";
            List<Cliente> clientes = new List<Cliente>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    clientes.Add(LerLinha(reader));
            }
            return clientes;
        }

        public Cliente BuscarPorId(int id)
        {
            string sql = "SELECT * FROM clientes WHERE cliente_id = @cliente_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cliente_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Cliente
        private Cliente LerLinha(MySqlDataReader reader)
        {
            return new Cliente
            {
                ClienteId = Convert.ToInt32(reader["cliente_id"]),
                Nome = reader["nome"].ToString(),
                TipoPessoa = reader["tipo_pessoa"].ToString(),
                Sexo = BancoHelper.TextoOuNulo(reader, "sexo"),
                CpfCnpj = reader["cpf_cnpj"].ToString(),
                Email = BancoHelper.TextoOuNulo(reader, "email"),
                Telefone = reader["telefone"].ToString(),
                DataNascimento = BancoHelper.DataOuNula(reader, "data_nascimento"),
                Logradouro = BancoHelper.TextoOuNulo(reader, "logradouro"),
                Numero = BancoHelper.TextoOuNulo(reader, "numero"),
                Bairro = BancoHelper.TextoOuNulo(reader, "bairro"),
                Cidade = BancoHelper.TextoOuNulo(reader, "cidade"),
                Estado = BancoHelper.TextoOuNulo(reader, "estado"),
                Cep = BancoHelper.TextoOuNulo(reader, "cep"),
                DataCadastro = Convert.ToDateTime(reader["data_cadastro"])
            };
        }
    }
}
