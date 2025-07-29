using RaymiMusic.AppWeb.Models;
using RaymiMusic.AppWeb.Services;
using RaymiMusic.Modelos;

public class PlaylistService : IPlaylistService
{
    private readonly HttpClient _http;
    public PlaylistService(HttpClient http) => _http = http;

    public async Task<IEnumerable<PlaylistDTO>> GetAllAsync()
    {
        return await _http
           .GetFromJsonAsync<IEnumerable<PlaylistDTO>>("api/ListasReproduccion")
           ?? Array.Empty<PlaylistDTO>();
    }

    public async Task<PlaylistDetailsVM?> GetByIdAsync(Guid id)
    {
        // 1) Consumimos la playlist completa con Artista y Album incluidos
        var lista = await _http
           .GetFromJsonAsync<ListaReproduccion>($"api/ListasReproduccion/{id}");

        if (lista == null)
            return null;

        // 2) Mapeamos a PlaylistDetailsVM
        var vm = new PlaylistDetailsVM
        {
            Id = lista.Id,
            Nombre = lista.Nombre,
            EsPublica = lista.EsPublica,
            Canciones = lista.CancionesEnListas.Select(cl => {
                var c = cl.Cancion!;
                return new SongDTO
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    ArtistaNombre = c.Artista?.NombreArtistico ?? "—",
                    AlbumNombre = c.Album?.Titulo ?? "—",
                    Duracion = c.Duracion,
                    RutaArchivo = c.RutaArchivo
                };
            }).ToList()
        };

        return vm;
    }

    public async Task<IEnumerable<ListaReproduccion>> GetListasUsuario(Guid userId)
    {
        // GET /api/ListasReproduccion/Usuario/{usuarioId}
        return await _http
            .GetFromJsonAsync<IEnumerable<ListaReproduccion>>($"api/ListasReproduccion/Usuario/{userId}")
            ?? Array.Empty<ListaReproduccion>();
    }
    public async Task<IEnumerable<ListaReproduccion>> GetPublicPlaylistsAsync()
    {
        // GET /api/ListasReproduccion/Publicas
        return await _http
            .GetFromJsonAsync<IEnumerable<ListaReproduccion>>("api/ListasReproduccion/Publicas")
            ?? Array.Empty<ListaReproduccion>();
    }

    public async Task CreateAsync(CreatePlaylistVM vm)
    {
        // POST /api/ListasReproduccion
        await _http.PostAsJsonAsync("api/ListasReproduccion", new
        {
            nombre = vm.Nombre,
            esPublica = vm.EsPublica,
            usuarioId = vm.UsuarioId
        });
    }



    public async Task AddSongAsync(Guid playlistId, Guid songId)
    {
        // POST /api/ListasReproduccion/{listaId}/AgregarCancion/{cancionId}
        var response = await _http.PostAsync(
            $"api/ListasReproduccion/{playlistId}/AgregarCancion/{songId}",
            null
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveSongAsync(Guid playlistId, Guid songId)
    {
        // Delete /api/ListasReproduccion/{listaId}/AgregarCancion/{cancionId}
        var response = await _http.DeleteAsync(
            $"api/ListasReproduccion/{playlistId}/RemoveSong/{songId}"
            
        );
        response.EnsureSuccessStatusCode();
    }
}
