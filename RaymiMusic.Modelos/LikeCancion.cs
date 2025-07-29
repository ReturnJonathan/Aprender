using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaymiMusic.Modelos
{
    public class LikeCancion
    {
        [Key] public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid CancionId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public Usuario? Usuario { get; set; }
        public Cancion? Cancion { get; set; }



    }
}