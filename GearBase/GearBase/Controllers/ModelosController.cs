using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/modelos")]
    public class ModelosController : CrudController<Modelo>
    {
        public ModelosController() : base(new ModeloDAO()) { }

        protected override void DefinirId(Modelo objeto, int id)
        {
            objeto.ModeloId = id;
        }
    }
}
