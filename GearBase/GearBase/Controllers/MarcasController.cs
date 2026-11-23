using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/marcas")]
    public class MarcasController : CrudController<Marca>
    {
        public MarcasController() : base(new MarcaDAO()) { }

        protected override void DefinirId(Marca objeto, int id)
        {
            objeto.MarcaId = id;
        }
    }
}
