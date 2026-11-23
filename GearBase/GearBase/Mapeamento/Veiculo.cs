namespace GearBase.Mapeamento
{
    public class Veiculo
    {
        public int VeiculoId { get; set; }
        public int ModeloId { get; set; }
        public int? ClienteId { get; set; } // dono do veículo; null quando é da concessionária
        public string Chassi { get; set; }
        public string Placa { get; set; }
        public int AnoFabricacao { get; set; }
        public int AnoModelo { get; set; }
        public string Cor { get; set; }
        public string Combustivel { get; set; } // Gasolina, Etanol, Flex, Diesel, Elétrico, Híbrido
        public string Transmissao { get; set; } // Manual, Automático, CVT, Automatizado
        public string Motorizacao { get; set; }
        public int? PotenciaCv { get; set; }
        public int Portas { get; set; } = 4;
        public string Tipo { get; set; } // Novo, Seminovo
        public string Status { get; set; } = "Disponível"; // Disponível, Reservado, Vendido, Em Manutenção
        public DateOnly DataEntrada { get; set; }

        private int _quilometragem;
        private decimal _precoCusto;
        private decimal _precoVenda;

        public int Quilometragem
        {
            get { return _quilometragem; }
            set
            {
                if (value < 0)
                    throw new Exception("A quilometragem não pode ser negativa");
                _quilometragem = value;
            }
        }

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
    }
}
