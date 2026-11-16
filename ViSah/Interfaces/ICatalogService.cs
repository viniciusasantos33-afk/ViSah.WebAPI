using ViSah.Data;
using ViSah.Models;
using ViSah.Services;

namespace ViSah.Interfaces;

public interface ICatalogService
{
    Task<object> Search(TrackQuery q);
    Task<Track?> GetTrack(int id);
    Task<object> Recommend(int moodId, int? genreId, int userId);
}