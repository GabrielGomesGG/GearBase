using GearBase.Mapeamento;

namespace GearBase.Interface
{
    // Chave primária composta (veiculo_id + opcional_id), por isso não usa IDAO<T>
    public interface IVeiculoOpcionalDAO
    {
        void Insert(VeiculoOpcional objeto);
        void Update(VeiculoOpcional objeto);
        void Delete(int veiculoId, int opcionalId);
        List<VeiculoOpcional> ListarTodos();
        List<VeiculoOpcional> ListarPorVeiculo(int veiculoId);
        VeiculoOpcional BuscarPorId(int veiculoId, int opcionalId);
    }
}
