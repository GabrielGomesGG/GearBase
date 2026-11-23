using GearBase.Mapeamento;

namespace GearBase.Interface
{
    // Herda o CRUD padrão e acrescenta um método específico
    public interface IItemOrdemServicoDAO : IDAO<ItemOrdemServico>
    {
        List<ItemOrdemServico> ListarPorOrdem(int ordemServicoId);
    }
}
