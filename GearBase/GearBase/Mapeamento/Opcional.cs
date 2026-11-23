namespace GearBase.Mapeamento
{
    public class Opcional
    {
        public int OpcionalId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; } // Conforto, Segurança, Estética, Tecnologia, Desempenho

        private decimal _precoTabela;

        public decimal PrecoTabela
        {
            get { return _precoTabela; }
            set
            {
                if (value < 0)
                    throw new Exception("O preço de tabela não pode ser negativo");
                _precoTabela = value;
            }
        }
    }
}
