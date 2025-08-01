﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaymiMusic.Modelos;

namespace RaymiMusic.AppWeb.Models
{
    public class AddSongVM
    {
        public Guid SongId { get; set; }

        [Required(ErrorMessage = "Seleccione una playlist")]
        public Guid PlaylistId { get; set; }

        public IEnumerable<ListaReproduccion> Playlists { get; set; }
            = new List<ListaReproduccion>();
    }

}
