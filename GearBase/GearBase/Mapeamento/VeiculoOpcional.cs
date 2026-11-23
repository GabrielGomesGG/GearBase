namespace GearBase.Mapeamento
{
    public class VeiculoOpcional
    {
        public int VeiculoId { get; set; }
        public int OpcionalId { get; set; }
        public bool InstaladoFabrica { get; set; } = true;

        private decimal _precoAplicado;

        public decimal PrecoAplicado
        {
            get { return _precoAplicado; }
            set
            {
                if (value < 0)
                    throw new Exception("O preço aplicado não pode ser negativo");
                _precoAplicado = value;
            }
        }
    }
}
