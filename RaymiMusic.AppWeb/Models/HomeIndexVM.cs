using System.Collections.Generic;
using RaymiMusic.Modelos;  // o el DTO que uses

namespace RaymiMusic.AppWeb.Models
{
    public class HomeIndexVM
    {
        public string Query { get; set; }
        public IEnumerable<Artista> Artistas { get; set; } = new List<Artista>();

        public IEnumerable<SongDTO> Songs { get; set; }
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();

    }
}
