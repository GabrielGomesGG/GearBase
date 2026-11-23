using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/fornecedores")]
    public class FornecedoresController : CrudController<Fornecedor>
    {
        public FornecedoresController() : base(new FornecedorDAO()) { }

        protected override void DefinirId(Fornecedor objeto, int id)
        {
            objeto.FornecedorId = id;
        }
    }
}
