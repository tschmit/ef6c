using System;
using System.Threading.Tasks;
using System.Linq;

using EF6 = System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

using EFC = Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // what makes EFC alias useless, but one have to, to reach extension methods like:
                                     //   - UseSqlServer


namespace EF6C {
    class Program {
        static async Task Main(string[] args) {
            string cs = "Data Source=.\\SQL140EXP;Initial Catalog=EF6C;Integrated Security=True;MultipleActiveResultSets=True";

            Console.WriteLine("EF6");
            using (EF6_Ctx ef6_ctx = new EF6_Ctx(cs)) {
                int c = ef6_ctx.Blogs.Count();
                Console.WriteLine($"{c}");
                ef6_ctx.Blogs.Add(new Blog { Title = $"Blog {c+1}"});
                await ef6_ctx.SaveChangesAsync();
            }
            Console.WriteLine("EFCore");
            using (EFC_Ctx efc_ctx = new EFC_Ctx(cs)) {
                Console.WriteLine($"{efc_ctx.Blogs.Count()}");
                Console.WriteLine((await efc_ctx.Blogs.OrderBy(b => b.Id).LastAsync())?.Title);
            }
        }
    }

    public class EF6_Ctx : EF6.DbContext {
        public EF6_Ctx(string cs) : base(cs) { }
        public EF6.DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(EF6.DbModelBuilder mb) {
            mb.Entity<Blog>().ToTable("Blogs");
            mb.Entity<Blog>().HasKey(e => e.Id);
            mb.Entity<Blog>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            mb.Entity<Blog>().Property(e => e.Title).HasColumnType("NVARCHAR").HasMaxLength(256);

            base.OnModelCreating(mb);
        }
    }

    public class EFC_Ctx : EFC.DbContext {
        public EFC_Ctx(string cs) {
            CS = cs;
        }

        public EFC.DbSet<Blog> Blogs { get; set; }

        public string CS { get; set; }
        protected override void OnConfiguring(EFC.DbContextOptionsBuilder ob) {
            ob.
                //UseLazyLoadingProxies().
                UseSqlServer(CS);

            base.OnConfiguring(ob);
        }

        protected override void OnModelCreating(ModelBuilder mb) {
            mb.Entity<Blog>().ToTable("Blogs");
            mb.Entity<Blog>().HasKey(e => e.Id);
            mb.Entity<Blog>().Property(e => e.Id).UseIdentityColumn(); // HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            mb.Entity<Blog>().Property(e => e.Title).HasColumnType("NVARCHAR").HasMaxLength(256);

            base.OnModelCreating(mb);
        }
    }

    public class Blog {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
