// Models/CancionEditVM.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaymiMusic.AppWeb.Models
{
    public class CancionEditVM
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        // Nuevo: para subir un archivo opcional
        public IFormFile? AudioFile { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un género.")]
        public Guid GeneroId { get; set; }

        public Guid? AlbumId { get; set; }

        public Guid ArtistaId { get; set; }

        // Este campo es solo de solo lectura para mostrar en la vista
        public TimeSpan DuracionActual { get; set; }
    }
}
