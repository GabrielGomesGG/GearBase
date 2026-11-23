using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/caixa")]
    public class CaixaController : CrudController<LancamentoCaixa>
    {
        public CaixaController() : base(new LancamentoCaixaDAO()) { }

        protected override void DefinirId(LancamentoCaixa objeto, int id)
        {
            objeto.LancamentoId = id;
        }
    }
}
