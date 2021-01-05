using ControleDeLeiloes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ControleDeLeiloes.Data
{
    public class ControleDeLeiloesDbContext : IdentityDbContext<Usuario>
    {
        public ControleDeLeiloesDbContext(DbContextOptions<ControleDeLeiloesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Produto> Produto { get; set; }
        public virtual DbSet<Lote> Lote { get; set; }
        public virtual DbSet<Leilao> Leilao { get; set; }
        public virtual DbSet<Leiloeiro> Leiloeiro { get; set; }
        public virtual DbSet<Anuncio> Anuncio { get; set; }
        public virtual DbSet<VendedorProibido> VendedorProibido { get; set; }
        public virtual DbSet<CategoriaAnuncio> CategoriaAnuncio { get; set; }
        public virtual DbSet<SubcategoriaAnuncio> SubcategoriaAnuncio { get; set; }
        public virtual DbSet<UrlAnuncio> UrlAnuncio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VendedorProibido>()
                .HasIndex(b => b.IdVendedor);
            base.OnModelCreating(modelBuilder);
        }

    }
}
