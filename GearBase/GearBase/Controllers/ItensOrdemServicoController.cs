using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/itens-ordem-servico")]
    public class ItensOrdemServicoController : CrudController<ItemOrdemServico>
    {
        public ItensOrdemServicoController() : base(new ItemOrdemServicoDAO()) { }

        protected override void DefinirId(ItemOrdemServico objeto, int id)
        {
            objeto.ItemOsId = id;
        }

        private readonly ItemOrdemServicoDAO itemDao = new ItemOrdemServicoDAO();

        // GET api/itens-ordem-servico/ordem/5  -> todos os itens da ordem 5
        [HttpGet("ordem/{ordemServicoId}")]
        public IActionResult ListarPorOrdem(int ordemServicoId)
        {
            return Ok(itemDao.ListarPorOrdem(ordemServicoId));
        }
    }
}
