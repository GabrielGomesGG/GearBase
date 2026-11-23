namespace GearBase.Mapeamento
{
    public class Funcionario
    {
        public int FuncionarioId { get; set; }
        public int CargoId { get; set; }
        public string Matricula { get; set; } // usada no login (a senha não sai da API)
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Sexo { get; set; } // M, F, Outro
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public DateOnly DataAdmissao { get; set; }
        public DateOnly? DataDemissao { get; set; }
        public string Status { get; set; } = "Ativo"; // Ativo, Férias, Afastado, Desligado
    }
}
