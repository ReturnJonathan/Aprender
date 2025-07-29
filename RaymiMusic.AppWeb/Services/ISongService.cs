using System.Collections.Generic;
using System.Threading.Tasks;
using RaymiMusic.AppWeb.Models;
using RaymiMusic.Modelos;

namespace RaymiMusic.AppWeb.Services
{
    public interface ISongService
    {
        Task<IEnumerable<SongDTO>> GetAllAsync();
        Task<IEnumerable<SongDTO>> SearchAsync(string query);
        Task<SongDTO?> GetByIdAsync(Guid id);
        Task CreateLikeAsync(LikeCancion like);
        Task DeleteLike(Guid idLike);

        Task<LikeCancion?> GetSongLike(Guid idUsuario, Guid idCancion);
        Task<IEnumerable<LikeCancion>> GetLikesForUser(Guid userId);



    }
}
