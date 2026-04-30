using DataSources;

var allAlbums = ListAlbumsData.ListAlbums;
var allArtists = ListArtistsData.ListArtists;

Console.Write("Rechercher un album : ");
var search = Console.ReadLine();

const int pageSize = 20;
var page = 0;
var filteredAlbums = allAlbums
    .Where(album => album.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
    .Join(allArtists,
        album => album.ArtistId,
        artist => artist.ArtistId,
        (album, artist) => new
        {
            album.AlbumId,
            album.Title,
            artist.ArtistId,
            ArtistName = artist.Name
        })
    .OrderBy(x => x.Title)
    .ThenByDescending(x => x.AlbumId);

var totalItems = filteredAlbums.Count();
var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

bool continuer = true;
while (continuer)
{
    var pageResult = filteredAlbums
        .Skip(page * pageSize)
        .Take(pageSize)
        .GroupBy(x => new { x.ArtistId, x.ArtistName })
        .Select(group => new
        {
            group.Key.ArtistId,
            group.Key.ArtistName,
            Albums = group.Select(x => $"Album n°{x.AlbumId} : {x.Title}")
        });

    Console.Clear();
    Console.WriteLine($"[{page + 1}/{totalPages}]");

    foreach (var group in pageResult)
    {
        Console.WriteLine($"\nArtiste : {group.ArtistName} ({group.ArtistId})");
        foreach (var albumToShow in group.Albums)
        {
            Console.WriteLine($"   {albumToShow}");
        }
    }

    Console.WriteLine($"\n[{page + 1}/{totalPages}]");
    Console.WriteLine("\n[P] Page précédente  [N] Page suivante  [Q] Quitter");
    var key = Console.ReadKey().Key;

    if (key == ConsoleKey.N && page < totalPages - 1) page++;
    else if (key == ConsoleKey.P && page > 0) page--;
    else if (key == ConsoleKey.Q) continuer = false;
}
