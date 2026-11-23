namespace GearBase.Mapeamento
{
    public class Venda
    {
        // Acima disso, o parcelamento precisa da liberação de um gerente
        public const int ParcelasSemLiberacao = 60;

        public int VendaId { get; set; }
        public int ClienteId { get; set; }
        public int FuncionarioId { get; set; }
        public int VeiculoId { get; set; }
        public DateTime DataVenda { get; set; } = DateTime.Now;
        public decimal ValorVeiculo { get; set; }
        public decimal? ValorEntrada { get; set; } = 0;
        public decimal? Desconto { get; set; } = 0;
        public decimal ValorTotal { get; set; }
        public string FormaPagamento { get; set; } // À Vista, Financiamento, Consórcio, Entrada + Financiamento, Cartão de Crédito
        public int? QuantidadeParcelas { get; set; } = 1;
        public decimal? TaxaJuros { get; set; } = 0;
        public string StatusVenda { get; set; } = "Concluída"; // Pendente, Concluída, Cancelada
        public string Observacoes { get; set; }

        /* Matrícula e senha do gerente que liberou o parcelamento.
           Só chega da tela, não é coluna do banco, e o VendasController apaga depois de conferir. */
        public Login Liberacao { get; set; }

        // Regra de negócio: valor total = valor do veículo - desconto
        public decimal CalcularValorTotal()
        {
            if (ValorVeiculo < 0)
                throw new Exception("O valor do veículo não pode ser negativo");
            if (QuantidadeParcelas != null && QuantidadeParcelas < 1)
                throw new Exception("A quantidade de parcelas deve ser pelo menos 1");

            decimal desconto = 0;
            if (Desconto != null)
                desconto = (decimal)Desconto;

            ValorTotal = ValorVeiculo - desconto;

            if (ValorTotal < 0)
                throw new Exception("O desconto não pode ser maior que o valor do veículo");

            return ValorTotal;
        }
    }
}
