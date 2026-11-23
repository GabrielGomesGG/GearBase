using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/veiculos")]
    public class VeiculosController : CrudController<Veiculo>
    {
        public VeiculosController() : base(new VeiculoDAO()) { }

        protected override void DefinirId(Veiculo objeto, int id)
        {
            objeto.VeiculoId = id;
        }
    }
}
