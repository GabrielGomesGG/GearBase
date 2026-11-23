using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/cargos")]
    public class CargosController : CrudController<Cargo>
    {
        public CargosController() : base(new CargoDAO()) { }

        protected override void DefinirId(Cargo objeto, int id)
        {
            objeto.CargoId = id;
        }
    }
}
