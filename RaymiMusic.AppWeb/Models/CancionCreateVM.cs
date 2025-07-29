// Models/CancionCreateVM.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaymiMusic.AppWeb.Models
{
    public class CancionCreateVM
    {
        [Required]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "Debes seleccionar un archivo de audio.")]
        public IFormFile AudioFile { get; set; } = null!;

        [Required(ErrorMessage = "Debes elegir un género.")]
        public Guid GeneroId { get; set; }

        public Guid? AlbumId { get; set; }
        public Guid? ArtistaId { get; set; }
    }
}
