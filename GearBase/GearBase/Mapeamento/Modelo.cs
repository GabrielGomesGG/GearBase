namespace GearBase.Mapeamento
{
    public class Modelo
    {
        public int ModeloId { get; set; }
        public int MarcaId { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; } // Sedan, Hatch, SUV, Picape, Coupé, Conversível, Van, Outro
        public int? AnoLancamento { get; set; }
    }
}
