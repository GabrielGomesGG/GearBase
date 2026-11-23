namespace GearBase.Mapeamento
{
    // Quem está logado: o funcionário e o que o cargo dele permite
    public class Usuario
    {
        public int FuncionarioId { get; set; }
        public string Matricula { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public bool PodeVender { get; set; }
        public bool PodeAbrirOs { get; set; }
        public bool PodeLiberar { get; set; }
    }
}
