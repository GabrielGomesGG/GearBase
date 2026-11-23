using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/funcionarios")]
    public class FuncionariosController : CrudController<Funcionario>
    {
        public FuncionariosController() : base(new FuncionarioDAO()) { }

        protected override void DefinirId(Funcionario objeto, int id)
        {
            objeto.FuncionarioId = id;
        }
    }
}
