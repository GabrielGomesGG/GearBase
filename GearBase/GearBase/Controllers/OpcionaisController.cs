using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/opcionais")]
    public class OpcionaisController : CrudController<Opcional>
    {
        public OpcionaisController() : base(new OpcionalDAO()) { }

        protected override void DefinirId(Opcional objeto, int id)
        {
            objeto.OpcionalId = id;
        }
    }
}
