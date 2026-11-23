using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/ordens-servico")]
    public class OrdensServicoController : CrudController<OrdemServico>
    {
        public OrdensServicoController() : base(new OrdemServicoDAO()) { }

        protected override void DefinirId(OrdemServico objeto, int id)
        {
            objeto.OrdemServicoId = id;
        }
    }
}
