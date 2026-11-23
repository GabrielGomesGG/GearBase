namespace GearBase.Mapeamento
{
    public class Peca
    {
        public int PecaId { get; set; }
        public int FornecedorId { get; set; }
        public string CodigoReferencia { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string LocalizacaoPrateleira { get; set; }

        private decimal _precoCusto;
        private decimal _precoVenda;
        private int _quantidadeEstoque;
        private int _estoqueMinimo = 5;

        public decimal PrecoCusto
        {
            get { return _precoCusto; }
            set
            {
                if (value < 0)
                    throw new Exception("O preço de custo não pode ser negativo");
                _precoCusto = value;
            }
        }

        public decimal PrecoVenda
        {
            get { return _precoVenda; }
            set
            {
                if (value < 0)
                    throw new Exception("O preço de venda não pode ser negativo");
                _precoVenda = value;
            }
        }

        public int QuantidadeEstoque
        {
            get { return _quantidadeEstoque; }
            set
            {
                if (value < 0)
                    throw new Exception("A quantidade em estoque não pode ser negativa");
                _quantidadeEstoque = value;
            }
        }

        public int EstoqueMinimo
        {
            get { return _estoqueMinimo; }
            set
            {
                if (value < 0)
                    throw new Exception("O estoque mínimo não pode ser negativo");
                _estoqueMinimo = value;
            }
        }
    }
}
