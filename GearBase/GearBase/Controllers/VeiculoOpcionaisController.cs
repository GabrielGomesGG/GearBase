using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    // Não herda de CrudController porque a chave é composta (veiculoId + opcionalId)
    [ApiController]
    [Route("api/veiculo-opcionais")]
    public class VeiculoOpcionaisController : ControllerBase
    {
        private readonly VeiculoOpcionalDAO dao = new VeiculoOpcionalDAO();

        // GET api/veiculo-opcionais
        [HttpGet]
        public IActionResult ListarTodos()
        {
            return Ok(dao.ListarTodos());
        }

        // GET api/veiculo-opcionais/veiculo/3  -> opcionais do veículo 3
        [HttpGet("veiculo/{veiculoId}")]
        public IActionResult ListarPorVeiculo(int veiculoId)
        {
            return Ok(dao.ListarPorVeiculo(veiculoId));
        }

        // GET api/veiculo-opcionais/3/7
        [HttpGet("{veiculoId}/{opcionalId}")]
        public IActionResult BuscarPorId(int veiculoId, int opcionalId)
        {
            VeiculoOpcional objeto = dao.BuscarPorId(veiculoId, opcionalId);

            if (objeto == null)
                throw new Exception("Registro não encontrado.");

            return Ok(objeto);
        }

        // POST api/veiculo-opcionais
        [HttpPost]
        public IActionResult Inserir(VeiculoOpcional objeto)
        {
            dao.Insert(objeto);
            return Ok(objeto);
        }

        // PUT api/veiculo-opcionais/3/7
        [HttpPut("{veiculoId}/{opcionalId}")]
        public IActionResult Atualizar(int veiculoId, int opcionalId, VeiculoOpcional objeto)
        {
            objeto.VeiculoId = veiculoId;
            objeto.OpcionalId = opcionalId;
            dao.Update(objeto);
            return Ok();
        }

        // DELETE api/veiculo-opcionais/3/7
        [HttpDelete("{veiculoId}/{opcionalId}")]
        public IActionResult Deletar(int veiculoId, int opcionalId)
        {
            dao.Delete(veiculoId, opcionalId);
            return Ok();
        }
    }
}
