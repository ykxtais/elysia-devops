namespace ElysiaAPI.Domain.Entity
{
    public class Vaga
    {
        public int Id { get; private set; }
        
        public string Status { get; set; } = "Livre";

        public int Numero { get; private set; }
        public string Patio { get; private set; } = "";

        private Vaga() { } 

        public Vaga(int numero, string patio)
        {
            DefinirLocalizacao(numero, patio);
        }

        public void AtualizarLocalizacao(int numero, string patio)
        {
            DefinirLocalizacao(numero, patio);
        }

        private void DefinirLocalizacao(int numero, string patio)
        {
            if (numero <= 0) throw new ArgumentException("Número da vaga deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(patio)) throw new ArgumentException("Pátio é obrigatório.");
            Numero = numero;
            Patio  = patio.Trim();
        }
    }
}