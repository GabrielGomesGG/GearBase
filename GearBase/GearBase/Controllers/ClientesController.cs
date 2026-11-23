using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/clientes")]
    public class ClientesController : CrudController<Cliente>
    {
        public ClientesController() : base(new ClienteDAO()) { }

        protected override void DefinirId(Cliente objeto, int id)
        {
            objeto.ClienteId = id;
        }
    }
}
