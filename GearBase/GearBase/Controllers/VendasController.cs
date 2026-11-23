using System.Security.Claims;
using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/vendas")]
    public class VendasController : CrudController<Venda>
    {
        public VendasController() : base(new VendaDAO()) { }

        protected override void DefinirId(Venda objeto, int id)
        {
            objeto.VendaId = id;
        }

        protected override void AntesDeSalvar(Venda venda, bool novo)
        {
            ConferirParcelamento(venda, novo);

            // A senha do gerente só serve para a conferência e não pode voltar na resposta
            venda.Liberacao = null;
        }

        /* Parcelamento acima de 60 meses precisa da liberação de um gerente (cargo com pode_liberar).
           Se quem está logado já é gerente, não pede senha. */
        private void ConferirParcelamento(Venda venda, bool novo)
        {
            int parcelas = 1;
            if (venda.QuantidadeParcelas != null)
                parcelas = (int)venda.QuantidadeParcelas;

            if (parcelas <= Venda.ParcelasSemLiberacao)
                return;

            // Editar uma venda que já tinha esse parcelamento não pede liberação de novo
            if (!novo)
            {
                Venda anterior = dao.BuscarPorId(venda.VendaId);
                if (anterior != null && anterior.QuantidadeParcelas == parcelas)
                    return;
            }

            // Id de quem está logado, guardado no cookie (veja o LoginController)
            FuncionarioDAO funcionarioDAO = new FuncionarioDAO();
            Usuario logado = funcionarioDAO.BuscarUsuario(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            Usuario gerente;
            if (logado != null && logado.PodeLiberar)
                gerente = logado;
            else if (venda.Liberacao == null)
                throw new Exception($"Parcelamento acima de {Venda.ParcelasSemLiberacao} meses precisa da liberação de um gerente.");
            else
                gerente = funcionarioDAO.Autenticar(venda.Liberacao.Matricula.Trim().ToUpper(), venda.Liberacao.Senha);

            if (gerente == null || !gerente.PodeLiberar)
                throw new Exception("Liberação recusada: matrícula ou senha incorreta, ou o cargo não libera parcelamento.");

            // Quem liberou fica registrado na própria venda
            string registro = $"Parcelamento em {parcelas} vezes liberado por {gerente.Nome} ({gerente.Matricula}).";
            if (venda.Observacoes == null || venda.Observacoes.Trim() == "")
                venda.Observacoes = registro;
            else
                venda.Observacoes = $"{venda.Observacoes.Trim()} {registro}";
        }
    }
}
