
namespace ElysiaAPI.Domain.Entity
{
    public class Usuario
    {
        public int Id { get; private set; }

        public string Nome  { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string Senha { get; private set; } = "";
        public string Cpf   { get; private set; } = "";

        private Usuario() { } 

        public Usuario(string nome, string email, string senha, string cpf)
        {
            AtualizarDadosBasicos(nome, email, cpf);
            DefinirSenha(senha);
        }

        public void AtualizarDadosBasicos(string nome, string email, string cpf)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.");
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF é obrigatório.");

            Nome  = nome.Trim();
            Email = email.Trim().ToLowerInvariant();
            Cpf   = cpf.Trim();
        }

        public void DefinirSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Trim().Length < 8)
                throw new ArgumentException("Senha deve ter ao menos 8 caracteres.");
            Senha = senha.Trim(); 
        }
    }
}
