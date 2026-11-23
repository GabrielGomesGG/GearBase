using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class FuncionarioDAO : IDAO<Funcionario>
    {
        // Senha de demonstração: todo funcionário novo começa com ela
        public const string SenhaPadrao = "gearbase";

        /* SHA2 (não vimos em sala) é uma função do MySQL que transforma a senha num código (hash) que não dá para
           desfazer. Assim o banco não guarda a senha de verdade, só esse código. */
        public int Insert(Funcionario funcionario)
        {
            string sql = @"INSERT INTO funcionarios (cargo_id, matricula, senha_hash, nome, cpf, sexo, email, telefone, data_nascimento, data_admissao, data_demissao, status)
                           VALUES (@cargo_id, @matricula, SHA2(@senha, 256), @nome, @cpf, @sexo, @email, @telefone, @data_nascimento, @data_admissao, @data_demissao, @status)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cargo_id", funcionario.CargoId);
                cmd.Parameters.AddWithValue("@matricula", funcionario.Matricula);
                cmd.Parameters.AddWithValue("@senha", SenhaPadrao);
                cmd.Parameters.AddWithValue("@nome", funcionario.Nome);
                cmd.Parameters.AddWithValue("@cpf", funcionario.Cpf);
                cmd.Parameters.AddWithValue("@sexo", BancoHelper.ParaBanco(funcionario.Sexo));
                cmd.Parameters.AddWithValue("@email", funcionario.Email);
                cmd.Parameters.AddWithValue("@telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@data_nascimento", BancoHelper.ParaBanco(funcionario.DataNascimento));
                cmd.Parameters.AddWithValue("@data_admissao", BancoHelper.ParaBanco(funcionario.DataAdmissao));
                cmd.Parameters.AddWithValue("@data_demissao", BancoHelper.ParaBanco(funcionario.DataDemissao));
                cmd.Parameters.AddWithValue("@status", funcionario.Status);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Funcionario funcionario)
        {
            string sql = @"UPDATE funcionarios SET
                    cargo_id = @cargo_id,
                    matricula = @matricula,
                    nome = @nome,
                    cpf = @cpf,
                    sexo = @sexo,
                    email = @email,
                    telefone = @telefone,
                    data_nascimento = @data_nascimento,
                    data_admissao = @data_admissao,
                    data_demissao = @data_demissao,
                    status = @status
                    WHERE funcionario_id = @funcionario_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@cargo_id", funcionario.CargoId);
                cmd.Parameters.AddWithValue("@nome", funcionario.Nome);
                cmd.Parameters.AddWithValue("@cpf", funcionario.Cpf);
                cmd.Parameters.AddWithValue("@sexo", BancoHelper.ParaBanco(funcionario.Sexo));
                cmd.Parameters.AddWithValue("@email", funcionario.Email);
                cmd.Parameters.AddWithValue("@telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@data_nascimento", BancoHelper.ParaBanco(funcionario.DataNascimento));
                cmd.Parameters.AddWithValue("@data_admissao", BancoHelper.ParaBanco(funcionario.DataAdmissao));
                cmd.Parameters.AddWithValue("@data_demissao", BancoHelper.ParaBanco(funcionario.DataDemissao));
                cmd.Parameters.AddWithValue("@status", funcionario.Status);
                cmd.Parameters.AddWithValue("@matricula", funcionario.Matricula);
                cmd.Parameters.AddWithValue("@funcionario_id", funcionario.FuncionarioId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum funcionário encontrado com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM funcionarios WHERE funcionario_id = @funcionario_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum funcionário encontrado com esse ID.");
            }
        }

        public List<Funcionario> ListarTodos()
        {
            string sql = "SELECT * FROM funcionarios";
            List<Funcionario> funcionarios = new List<Funcionario>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    funcionarios.Add(LerLinha(reader));
            }
            return funcionarios;
        }

        public Funcionario BuscarPorId(int id)
        {
            string sql = "SELECT * FROM funcionarios WHERE funcionario_id = @funcionario_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        /* O cargo diz o que o funcionário pode fazer (colunas pode_vender, pode_abrir_os e pode_financeiro da tabela cargos).
           Num cadastro novo, o funcionário também precisa estar ativo. */
        public void ConferirPermissao(int funcionarioId, string permissao, string acao, bool cadastroNovo)
        {
            string sql = @"SELECT f.nome, f.status, c.nome AS cargo, c.pode_vender, c.pode_abrir_os, c.pode_financeiro
                           FROM funcionarios f
                           JOIN cargos c ON c.cargo_id = f.cargo_id
                           WHERE f.funcionario_id = @funcionario_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", funcionarioId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("Funcionário não encontrado.");

                    string nome = reader["nome"].ToString();
                    string status = reader["status"].ToString();

                    if (!Convert.ToBoolean(reader[permissao]))
                        throw new Exception($"{nome} é {reader["cargo"].ToString()} e não tem permissão para {acao}.");
                    if (cadastroNovo && status != "Ativo")
                        throw new Exception($"{nome} está com a situação {status} e não pode {acao} agora.");
                }
            }
        }

        /* Login: confere matrícula e senha. A senha digitada passa pelo mesmo SHA2 (veja o Insert) e o código
           é comparado com o que está guardado. Funcionário desligado não entra. Devolve null quando não confere. */
        public Usuario Autenticar(string matricula, string senha)
        {
            string sql = @"SELECT f.funcionario_id, f.matricula, f.nome, c.nome AS cargo, c.pode_vender, c.pode_abrir_os, c.pode_liberar
                           FROM funcionarios f
                           JOIN cargos c ON c.cargo_id = f.cargo_id
                           WHERE f.matricula = @matricula AND f.senha_hash = SHA2(@senha, 256) AND f.status <> 'Desligado'";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@matricula", matricula);
                cmd.Parameters.AddWithValue("@senha", senha);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerUsuario(reader);
                }
            }
            return null;
        }

        // Dados de quem está logado, lidos de novo a cada pedido (o cargo pode ter mudado)
        public Usuario BuscarUsuario(int funcionarioId)
        {
            string sql = @"SELECT f.funcionario_id, f.matricula, f.nome, c.nome AS cargo, c.pode_vender, c.pode_abrir_os, c.pode_liberar
                           FROM funcionarios f
                           JOIN cargos c ON c.cargo_id = f.cargo_id
                           WHERE f.funcionario_id = @funcionario_id AND f.status <> 'Desligado'";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@funcionario_id", funcionarioId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerUsuario(reader);
                }
            }
            return null;
        }

        // Converte uma linha do SELECT de login em um objeto Usuario
        private Usuario LerUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                Matricula = reader["matricula"].ToString(),
                Nome = reader["nome"].ToString(),
                Cargo = reader["cargo"].ToString(),
                PodeVender = Convert.ToBoolean(reader["pode_vender"]),
                PodeAbrirOs = Convert.ToBoolean(reader["pode_abrir_os"]),
                PodeLiberar = Convert.ToBoolean(reader["pode_liberar"])
            };
        }

        // Converte uma linha do banco em um objeto Funcionario
        private Funcionario LerLinha(MySqlDataReader reader)
        {
            return new Funcionario
            {
                FuncionarioId = Convert.ToInt32(reader["funcionario_id"]),
                CargoId = Convert.ToInt32(reader["cargo_id"]),
                Matricula = reader["matricula"].ToString(),
                Nome = reader["nome"].ToString(),
                Cpf = reader["cpf"].ToString(),
                Sexo = BancoHelper.TextoOuNulo(reader, "sexo"),
                Email = reader["email"].ToString(),
                Telefone = reader["telefone"].ToString(),
                DataNascimento = BancoHelper.DataOuNula(reader, "data_nascimento"),
                DataAdmissao = BancoHelper.LerData(reader, "data_admissao"),
                DataDemissao = BancoHelper.DataOuNula(reader, "data_demissao"),
                Status = reader["status"].ToString()
            };
        }
    }
}
