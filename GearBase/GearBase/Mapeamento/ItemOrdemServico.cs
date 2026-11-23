namespace GearBase.Mapeamento
{
    public class ItemOrdemServico
    {
        public int ItemOsId { get; set; }
        public int OrdemServicoId { get; set; }
        public int PecaId { get; set; }
        public decimal? Desconto { get; set; } = 0;
        public decimal Subtotal { get; set; }

        private int _quantidade;
        private decimal _precoUnitario;

        public int Quantidade
        {
            get { return _quantidade; }
            set
            {
                if (value <= 0)
                    throw new Exception("A quantidade deve ser maior que zero");
                _quantidade = value;
            }
        }

        public decimal PrecoUnitario
        {
            get { return _precoUnitario; }
            set
            {
                if (value < 0)
                    throw new Exception("O preço unitário não pode ser negativo");
                _precoUnitario = value;
            }
        }

        // Regra de negócio: subtotal = quantidade x preço unitário - desconto
        public decimal CalcularSubtotal()
        {
            decimal desconto = 0;
            if (Desconto != null)
                desconto = (decimal)Desconto;

            if (desconto < 0)
                throw new Exception("O desconto não pode ser negativo");

            Subtotal = (Quantidade * PrecoUnitario) - desconto;

            if (Subtotal < 0)
                throw new Exception("O desconto não pode ser maior que o valor do item");

            return Subtotal;
        }
    }
}
