using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/pecas")]
    public class PecasController : CrudController<Peca>
    {
        public PecasController() : base(new PecaDAO()) { }

        protected override void DefinirId(Peca objeto, int id)
        {
            objeto.PecaId = id;
        }
    }
}
