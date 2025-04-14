using GameStore.Models;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace GameStore.Services
{
  public class GameService
  {
    private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "games.json");
    private readonly string _blobBaseUrl = "https://gameblob.blob.core.windows.net/images/";
    private readonly string _sasToken;

    public List<Game> GetAllGames()
    {
        var jsonData = File.ReadAllText(_filePath);
        var gameList = JsonSerializer.Deserialize<List<Game>>(jsonData) ?? new List<Game>();

        foreach (var game in gameList)
        {
            var blobName = Path.GetFileName(game.Cover);

            game.Cover = GetSecureImageUrl(blobName);
        }

      return gameList;

    }


    public GameService()
    {
        //hämtar sas-token från key vault
        var keyVaultUrl = "https://gamestorage-key.vault.azure.net/";
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        _sasToken = secretClient.GetSecret("ImageContainerReadSASToken").Value.Value;

    }

    public string GetSecureImageUrl(string blobName)
    {
        return $"{_blobBaseUrl}{blobName}?{_sasToken}";
    }
  }
}
