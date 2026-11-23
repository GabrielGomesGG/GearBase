namespace GearBase.Mapeamento
{
    public class Cargo
    {
        public int CargoId { get; set; }
        public string Nome { get; set; }
        public bool PodeVender { get; set; } // registra vendas e acompanha test drives
        public bool PodeAbrirOs { get; set; } // abre ordens de serviço
        public bool PodeLiberar { get; set; } // libera parcelamento acima do limite (gerente)
        public bool PodeFinanceiro { get; set; } // faz lançamentos no caixa

        private decimal _salarioBase;
        private decimal _comissaoPercentual;

        public decimal SalarioBase
        {
            get { return _salarioBase; }
            set
            {
                if (value < 0)
                    throw new Exception("O salário base não pode ser negativo");
                _salarioBase = value;
            }
        }

        public decimal ComissaoPercentual
        {
            get { return _comissaoPercentual; }
            set
            {
                if (value < 0)
                    throw new Exception("A comissão não pode ser negativa");
                _comissaoPercentual = value;
            }
        }
    }
}
