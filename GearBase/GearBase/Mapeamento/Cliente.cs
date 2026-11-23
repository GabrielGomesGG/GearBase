namespace GearBase.Mapeamento
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string TipoPessoa { get; set; } = "PF"; // PF, PJ
        public string Sexo { get; set; } // M, F, Outro
        public string CpfCnpj { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Cep { get; set; }
        public DateTime DataCadastro { get; set; } // preenchida pelo ClienteDAO no cadastro
    }
}
