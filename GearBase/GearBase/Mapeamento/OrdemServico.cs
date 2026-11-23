namespace GearBase.Mapeamento
{
    public class OrdemServico
    {
        public int OrdemServicoId { get; set; }
        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public int FuncionarioId { get; set; }
        public DateTime DataAbertura { get; set; } = DateTime.Now;
        public DateTime? DataConclusao { get; set; }
        public string DescricaoProblema { get; set; }
        public string DiagnosticoTecnico { get; set; }
        public decimal ValorTotal { get; set; }
        public string FormaPagamento { get; set; } // À Vista, Cartão de Crédito, Faturado, Pix
        public string Status { get; set; } = "Aberto"; // Aberto, Em Andamento, Aguardando Peça, Concluído, Cancelado

        private int _quilometragemEntrada;
        private decimal _valorMaoDeObra;
        private decimal _valorPecas;

        public int QuilometragemEntrada
        {
            get { return _quilometragemEntrada; }
            set
            {
                if (value < 0)
                    throw new Exception("A quilometragem de entrada não pode ser negativa");
                _quilometragemEntrada = value;
            }
        }

        public decimal ValorMaoDeObra
        {
            get { return _valorMaoDeObra; }
            set
            {
                if (value < 0)
                    throw new Exception("O valor da mão de obra não pode ser negativo");
                _valorMaoDeObra = value;
            }
        }

        public decimal ValorPecas
        {
            get { return _valorPecas; }
            set
            {
                if (value < 0)
                    throw new Exception("O valor das peças não pode ser negativo");
                _valorPecas = value;
            }
        }

        // Regra de negócio: valor total = mão de obra + peças
        public decimal CalcularValorTotal()
        {
            ValorTotal = ValorMaoDeObra + ValorPecas;
            return ValorTotal;
        }
    }
}
