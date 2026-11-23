using GearBase.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    // Classe base genérica: escreve o CRUD uma única vez para todas as tabelas
    [ApiController]
    public abstract class CrudController<T> : ControllerBase
    {
        protected readonly IDAO<T> dao;

        protected CrudController(IDAO<T> dao)
        {
            this.dao = dao;
        }

        // Cada controller filho diz qual propriedade é o ID
        protected abstract void DefinirId(T objeto, int id);

        // Ponto para um controller filho conferir algo antes de gravar (o padrão é não fazer nada)
        protected virtual void AntesDeSalvar(T objeto, bool novo) { }

        // GET api/{rota}
        [HttpGet]
        public IActionResult ListarTodos()
        {
            return Ok(dao.ListarTodos());
        }

        // GET api/{rota}/5
        [HttpGet("{id}")]
        public IActionResult BuscarPorId(int id)
        {
            T objeto = dao.BuscarPorId(id);

            if (objeto == null)
                throw new Exception("Registro não encontrado.");

            return Ok(objeto);
        }

        // POST api/{rota}
        [HttpPost]
        public IActionResult Inserir(T objeto)
        {
            AntesDeSalvar(objeto, true);
            int id = dao.Insert(objeto);
            DefinirId(objeto, id);
            return Ok(objeto);
        }

        // PUT api/{rota}/5
        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, T objeto)
        {
            DefinirId(objeto, id);
            AntesDeSalvar(objeto, false);
            dao.Update(objeto);
            return Ok(objeto);
        }

        // DELETE api/{rota}/5
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            dao.Delete(id);
            return Ok();
        }
    }
}
