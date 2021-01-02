
namespace ControleDeLeiloes.Models
{
    public class SubcategoriaAnuncio
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public CategoriaAnuncio CategoriaAnuncio { get; set; }
        public int CategoriaAnuncioId { get; set; }
    }
}
