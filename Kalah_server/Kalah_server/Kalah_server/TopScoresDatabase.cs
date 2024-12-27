using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Kalah_server
{
    public static class TopScoresPlayersDatabase
    {
        private static readonly string FilePath = "TopScoresPlayers.txt";

        static TopScoresPlayersDatabase()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close(); // Создать файл, если он отсутствует
            }
        }

        public static void SaveScore(string playerName, int score)
        {
            var scores = File.ReadAllLines(FilePath).ToList();
            scores.Add($"{playerName},{score}");
            File.WriteAllLines(FilePath, scores.OrderByDescending(s => int.Parse(s.Split(',')[1])).Take(5));
        }

        public static string[] GetTopScores()
        {
            if (!File.Exists(FilePath)) return Array.Empty<string>();
            return File.ReadAllLines(FilePath).Take(5).ToArray();
        }
    }

    public static class TopScoresComputerDatabase
    {
        private static readonly string FilePath = "TopScoresComputer.txt";

        static TopScoresComputerDatabase()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close(); // Создать файл, если он отсутствует
            }
        }

        public static void SaveScore(string playerName, int score)
        {
            var scores = File.ReadAllLines(FilePath).ToList();
            scores.Add($"{playerName},{score}");
            File.WriteAllLines(FilePath, scores.OrderByDescending(s => int.Parse(s.Split(',')[1])).Take(5));
        }

        public static string[] GetTopScores()
        {
            if (!File.Exists(FilePath)) return Array.Empty<string>();
            return File.ReadAllLines(FilePath).Take(5).ToArray();
        }
    }

}
