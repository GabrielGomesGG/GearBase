namespace GearBase.Mapeamento
{
    public class LancamentoCaixa
    {
        public int LancamentoId { get; set; }
        public int FuncionarioId { get; set; }
        public int? VendaId { get; set; } // preenchido só nos lançamentos que vieram de uma venda
        public int? OrdemServicoId { get; set; } // preenchido só nos que vieram de uma OS
        public string Tipo { get; set; } // Entrada, Saída
        public string Categoria { get; set; } // Venda de veículo, Ordem de serviço, Compra de peças, Salários, Despesas fixas, Impostos, Outros
        public string Descricao { get; set; }
        public string FormaPagamento { get; set; }
        public DateTime DataLancamento { get; set; }

        private decimal _valor;

        public decimal Valor
        {
            get { return _valor; }
            set
            {
                if (value <= 0)
                    throw new Exception("O valor do lançamento deve ser maior que zero");
                _valor = value;
            }
        }
    }
}
