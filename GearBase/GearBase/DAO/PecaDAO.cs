using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class PecaDAO : IDAO<Peca>
    {
        public int Insert(Peca peca)
        {
            string sql = @"INSERT INTO pecas (fornecedor_id, codigo_referencia, nome, descricao, preco_custo, preco_venda, quantidade_estoque, estoque_minimo, localizacao_prateleira)
                           VALUES (@fornecedor_id, @codigo_referencia, @nome, @descricao, @preco_custo, @preco_venda, @quantidade_estoque, @estoque_minimo, @localizacao_prateleira)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@fornecedor_id", peca.FornecedorId);
                cmd.Parameters.AddWithValue("@codigo_referencia", peca.CodigoReferencia);
                cmd.Parameters.AddWithValue("@nome", peca.Nome);
                cmd.Parameters.AddWithValue("@descricao", BancoHelper.ParaBanco(peca.Descricao));
                cmd.Parameters.AddWithValue("@preco_custo", peca.PrecoCusto);
                cmd.Parameters.AddWithValue("@preco_venda", peca.PrecoVenda);
                cmd.Parameters.AddWithValue("@quantidade_estoque", peca.QuantidadeEstoque);
                cmd.Parameters.AddWithValue("@estoque_minimo", peca.EstoqueMinimo);
                cmd.Parameters.AddWithValue("@localizacao_prateleira", BancoHelper.ParaBanco(peca.LocalizacaoPrateleira));
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public void Update(Peca peca)
        {
            string sql = @"UPDATE pecas SET
                    fornecedor_id = @fornecedor_id,
                    codigo_referencia = @codigo_referencia,
                    nome = @nome,
                    descricao = @descricao,
                    preco_custo = @preco_custo,
                    preco_venda = @preco_venda,
                    quantidade_estoque = @quantidade_estoque,
                    estoque_minimo = @estoque_minimo,
                    localizacao_prateleira = @localizacao_prateleira
                    WHERE peca_id = @peca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@fornecedor_id", peca.FornecedorId);
                cmd.Parameters.AddWithValue("@codigo_referencia", peca.CodigoReferencia);
                cmd.Parameters.AddWithValue("@nome", peca.Nome);
                cmd.Parameters.AddWithValue("@descricao", BancoHelper.ParaBanco(peca.Descricao));
                cmd.Parameters.AddWithValue("@preco_custo", peca.PrecoCusto);
                cmd.Parameters.AddWithValue("@preco_venda", peca.PrecoVenda);
                cmd.Parameters.AddWithValue("@quantidade_estoque", peca.QuantidadeEstoque);
                cmd.Parameters.AddWithValue("@estoque_minimo", peca.EstoqueMinimo);
                cmd.Parameters.AddWithValue("@localizacao_prateleira", BancoHelper.ParaBanco(peca.LocalizacaoPrateleira));
                cmd.Parameters.AddWithValue("@peca_id", peca.PecaId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma peça encontrada com esse ID.");
            }
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM pecas WHERE peca_id = @peca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@peca_id", id);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhuma peça encontrada com esse ID.");
            }
        }

        public List<Peca> ListarTodos()
        {
            string sql = "SELECT * FROM pecas";
            List<Peca> pecas = new List<Peca>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    pecas.Add(LerLinha(reader));
            }
            return pecas;
        }

        public Peca BuscarPorId(int id)
        {
            string sql = "SELECT * FROM pecas WHERE peca_id = @peca_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@peca_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        // Converte uma linha do banco em um objeto Peca
        private Peca LerLinha(MySqlDataReader reader)
        {
            return new Peca
            {
                PecaId = Convert.ToInt32(reader["peca_id"]),
                FornecedorId = Convert.ToInt32(reader["fornecedor_id"]),
                CodigoReferencia = reader["codigo_referencia"].ToString(),
                Nome = reader["nome"].ToString(),
                Descricao = BancoHelper.TextoOuNulo(reader, "descricao"),
                PrecoCusto = Convert.ToDecimal(reader["preco_custo"]),
                PrecoVenda = Convert.ToDecimal(reader["preco_venda"]),
                QuantidadeEstoque = Convert.ToInt32(reader["quantidade_estoque"]),
                EstoqueMinimo = Convert.ToInt32(reader["estoque_minimo"]),
                LocalizacaoPrateleira = BancoHelper.TextoOuNulo(reader, "localizacao_prateleira")
            };
        }
    }
}
