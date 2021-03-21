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

        public virtual DbSet<Produto> Produto { get; set; }
        public virtual DbSet<Lote> Lote { get; set; }
        public virtual DbSet<LoteProduto> LoteProduto { get; set; }
        public virtual DbSet<Anuncio> Anuncio { get; set; }
        public virtual DbSet<VendedorProibido> VendedorProibido { get; set; }
        public virtual DbSet<Foto> Foto { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Leilao> Leilao { get; set; }
        public virtual DbSet<Leiloeiro> Leiloeiro { get; set; }
        public virtual DbSet<CategoriaAnuncio> CategoriaAnuncio { get; set; }
        public virtual DbSet<SubcategoriaAnuncio> SubcategoriaAnuncio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoteProduto>()
                .HasKey(sc => new { sc.LoteId, sc.ProdtoId });
            modelBuilder.Entity<VendedorProibido>()
                .HasIndex(b => b.OlxIdVendedor);
            base.OnModelCreating(modelBuilder);
        }

    }
}
