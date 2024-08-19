namespace _3LargeMergeApp;

public static class CreateFiles
{
    public static async Task GenerateFilesAsync(IProgress<(int fileIndex, int percent)> progress)
    {
        var random = new Random();
        var startDate = new DateTime(2005, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var tasks = new List<Task>
        {
            CreateFileAsync(0, @"C:\dev\temp\temp\files\File1.csv", random, startDate, endDate, 10_000_000, progress),
            CreateFileAsync(1, @"C:\dev\temp\temp\files\File2.csv", random, startDate, endDate, 10_200_000, progress),
            CreateFileAsync(2, @"C:\dev\temp\temp\files\File3.csv", random, startDate, endDate, 10_300_000, progress)
        };

        await Task.WhenAll(tasks);
    }

    private static async Task CreateFileAsync(int fileIndex, string filePath, Random random, DateTime startDate, DateTime endDate, int lines, IProgress<(int fileIndex, int percent)> progress)
    {
        using var writer = new StreamWriter(filePath);
        for (int i = 0; i < lines; i++)
        {
            var randomDate = GetRandomDate(random, startDate, endDate);
            var randomInt = random.Next(1000, 10000);
            await writer.WriteLineAsync($"{randomDate},{randomInt}");
            progress?.Report((fileIndex, (i + 1) * 100 / lines));
        }
        await writer.FlushAsync();
    }

    private static DateTime GetRandomDate(Random random, DateTime startDate, DateTime endDate)
    {
        int range = (endDate - startDate).Days;
        var randomDate = startDate.AddDays(random.Next(range));
        var randomTime = new TimeSpan(random.Next(0, 24), random.Next(0, 60), random.Next(0, 60));
        return randomDate.Add(randomTime);
    }
}