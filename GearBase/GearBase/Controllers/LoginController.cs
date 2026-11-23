using System.Security.Claims;
using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    /* Login com cookie (não vimos em sala; a configuração está no Program.cs).
       O SignInAsync grava o cookie e o SignOutAsync apaga. Os dois são assíncronos: o await espera eles terminarem,
       e por isso esses métodos devolvem async Task<IActionResult>.
       Claim é uma informação sobre quem entrou; aqui guardo só o id do funcionário. A ClaimsIdentity junta os claims
       e o ClaimsPrincipal é o usuário logado que vai dentro do cookie. Nos próximos pedidos, o User.FindFirstValue
       lê esse id de volta. O [AllowAnonymous] libera o endpoint sem login (todos os outros exigem). */
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly FuncionarioDAO dao = new FuncionarioDAO();

        // Entrar: se matrícula e senha conferem, o navegador recebe um cookie que vale para os próximos pedidos
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Entrar(Login login)
        {
            Usuario usuario = dao.Autenticar(login.Matricula.Trim().ToUpper(), login.Senha);
            if (usuario == null)
                throw new Exception("Matrícula ou senha incorreta.");

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.FuncionarioId.ToString()));

            ClaimsIdentity identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identidade));
            return Ok(usuario);
        }

        // Quem está logado agora
        [HttpGet]
        public IActionResult Atual()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Usuario usuario = dao.BuscarUsuario(id);

            // Funcionário desligado depois de entrar: o 401 (Unauthorized) faz a tela voltar para o login
            if (usuario == null)
                return Unauthorized();
            return Ok(usuario);
        }

        // Sair: apaga o cookie
        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
