namespace GearBase.Mapeamento
{
    public class TestDrive
    {
        public int TestDriveId { get; set; }
        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public int FuncionarioId { get; set; }
        public DateTime DataAgendamento { get; set; }
        public string CnhCliente { get; set; }
        public DateOnly ValidadeCnh { get; set; }
        public int? KmInicial { get; set; }
        public int? KmFinal { get; set; }
        public string Status { get; set; } = "Agendado"; // Agendado, Em Andamento, Realizado, Cancelado
        public string Observacoes { get; set; }

        private int? _avaliacaoCliente;

        public int? AvaliacaoCliente
        {
            get { return _avaliacaoCliente; }
            set
            {
                if (value != null && (value < 1 || value > 5))
                    throw new Exception("A avaliação deve ser entre 1 e 5");
                _avaliacaoCliente = value;
            }
        }
    }
}
