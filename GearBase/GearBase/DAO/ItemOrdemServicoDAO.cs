using GearBase.DataBase;
using GearBase.Interface;
using GearBase.Mapeamento;
using MySql.Data.MySqlClient;

namespace GearBase.DAO
{
    public class ItemOrdemServicoDAO : IItemOrdemServicoDAO
    {
        // Usado para manter o valor_pecas da OS igual à soma dos itens
        private readonly OrdemServicoDAO ordemServicoDao = new OrdemServicoDAO();

        public int Insert(ItemOrdemServico itemOrdemServico)
        {
            itemOrdemServico.CalcularSubtotal();

            string sql = @"INSERT INTO itens_ordem_servico (ordem_servico_id, peca_id, quantidade, preco_unitario, desconto, subtotal)
                           VALUES (@ordem_servico_id, @peca_id, @quantidade, @preco_unitario, @desconto, @subtotal)";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", itemOrdemServico.OrdemServicoId);
                cmd.Parameters.AddWithValue("@peca_id", itemOrdemServico.PecaId);
                cmd.Parameters.AddWithValue("@quantidade", itemOrdemServico.Quantidade);
                cmd.Parameters.AddWithValue("@preco_unitario", itemOrdemServico.PrecoUnitario);
                cmd.Parameters.AddWithValue("@desconto", BancoHelper.ParaBanco(itemOrdemServico.Desconto));
                cmd.Parameters.AddWithValue("@subtotal", itemOrdemServico.Subtotal);
                cmd.ExecuteNonQuery();
                int id = (int)cmd.LastInsertedId;

                ordemServicoDao.RecalcularValores(itemOrdemServico.OrdemServicoId);
                return id;
            }
        }

        public void Update(ItemOrdemServico itemOrdemServico)
        {
            itemOrdemServico.CalcularSubtotal();

            // Guarda a OS antiga: se o item mudar de OS, as duas precisam ser recalculadas
            int ordemAntiga = 0;
            ItemOrdemServico antigo = BuscarPorId(itemOrdemServico.ItemOsId);
            if (antigo != null)
                ordemAntiga = antigo.OrdemServicoId;

            string sql = @"UPDATE itens_ordem_servico SET
                    ordem_servico_id = @ordem_servico_id,
                    peca_id = @peca_id,
                    quantidade = @quantidade,
                    preco_unitario = @preco_unitario,
                    desconto = @desconto,
                    subtotal = @subtotal
                    WHERE item_os_id = @item_os_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", itemOrdemServico.OrdemServicoId);
                cmd.Parameters.AddWithValue("@peca_id", itemOrdemServico.PecaId);
                cmd.Parameters.AddWithValue("@quantidade", itemOrdemServico.Quantidade);
                cmd.Parameters.AddWithValue("@preco_unitario", itemOrdemServico.PrecoUnitario);
                cmd.Parameters.AddWithValue("@desconto", BancoHelper.ParaBanco(itemOrdemServico.Desconto));
                cmd.Parameters.AddWithValue("@subtotal", itemOrdemServico.Subtotal);
                cmd.Parameters.AddWithValue("@item_os_id", itemOrdemServico.ItemOsId);

                int linhas = cmd.ExecuteNonQuery();
                if (linhas == 0)
                    throw new Exception("Nenhum item encontrado com esse ID.");
            }

            ordemServicoDao.RecalcularValores(itemOrdemServico.OrdemServicoId);
            if (ordemAntiga != 0 && ordemAntiga != itemOrdemServico.OrdemServicoId)
                ordemServicoDao.RecalcularValores(ordemAntiga);
        }

        public void Delete(int id)
        {
            // Precisa saber de qual OS o item era antes de apagar
            ItemOrdemServico item = BuscarPorId(id);
            if (item == null)
                throw new Exception("Nenhum item encontrado com esse ID.");

            string sql = "DELETE FROM itens_ordem_servico WHERE item_os_id = @item_os_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@item_os_id", id);
                cmd.ExecuteNonQuery();
            }

            ordemServicoDao.RecalcularValores(item.OrdemServicoId);
        }

        public List<ItemOrdemServico> ListarTodos()
        {
            string sql = "SELECT * FROM itens_ordem_servico";
            List<ItemOrdemServico> itens = new List<ItemOrdemServico>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    itens.Add(LerLinha(reader));
            }
            return itens;
        }

        public ItemOrdemServico BuscarPorId(int id)
        {
            string sql = "SELECT * FROM itens_ordem_servico WHERE item_os_id = @item_os_id";

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@item_os_id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return LerLinha(reader);
                }
            }
            return null;
        }

        public List<ItemOrdemServico> ListarPorOrdem(int ordemServicoId)
        {
            string sql = "SELECT * FROM itens_ordem_servico WHERE ordem_servico_id = @ordem_servico_id";
            List<ItemOrdemServico> itens = new List<ItemOrdemServico>();

            using (var conexao = Conexao.Conectar())
            using (var cmd = new MySqlCommand(sql, conexao))
            {
                cmd.Parameters.AddWithValue("@ordem_servico_id", ordemServicoId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        itens.Add(LerLinha(reader));
                }
            }
            return itens;
        }

        // Converte uma linha do banco em um objeto ItemOrdemServico
        private ItemOrdemServico LerLinha(MySqlDataReader reader)
        {
            return new ItemOrdemServico
            {
                ItemOsId = Convert.ToInt32(reader["item_os_id"]),
                OrdemServicoId = Convert.ToInt32(reader["ordem_servico_id"]),
                PecaId = Convert.ToInt32(reader["peca_id"]),
                Quantidade = Convert.ToInt32(reader["quantidade"]),
                PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                Desconto = BancoHelper.DecimalOuNulo(reader, "desconto"),
                Subtotal = Convert.ToDecimal(reader["subtotal"])
            };
        }
    }
}
