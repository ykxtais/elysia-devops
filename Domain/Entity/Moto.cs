using ElysiaAPI.Domain.ValueObjects;

namespace ElysiaAPI.Domain.Entity
{
    public class Moto
    {
        public int Id { get; private set; }
        public Placa Placa { get; private set; }
        public string Marca { get; private set; } = "";
        public string Modelo { get; private set; } = "";
        public int Ano { get; private set; }

        private Moto() { } 

        public Moto(Placa placa, string marca, string modelo, int ano)
        {
            DefinirPlaca(placa);
            AtualizarDadosBasicos(marca, modelo, ano);
        }

        public void DefinirPlaca(Placa placa) => Placa = placa;

        public void AtualizarDadosBasicos(string marca, string modelo, int ano)
        {
            if (string.IsNullOrWhiteSpace(marca)) throw new ArgumentException("Marca é obrigatória.");
            if (string.IsNullOrWhiteSpace(modelo)) throw new ArgumentException("Modelo é obrigatório.");
            var anoMax = DateTime.UtcNow.Year + 1;
            if (ano < 1885 || ano > anoMax) throw new ArgumentException("Ano inválido.");
            Marca = marca.Trim();
            Modelo = modelo.Trim();
            Ano = ano;
        }
    }
}