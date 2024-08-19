namespace _3LargeMergeApp;

public static class ReadAndMergeFiles
{
    private const int ChunkSize = 10000; // Define the size of each chunk

    public static async Task ReadAndMergeFilesAsync()
    {
        var chunkFiles = await CreateSortedChunksAsync(new[]
        {
            @"C:\dev\temp\temp\files\File1.csv",
            @"C:\dev\temp\temp\files\File2.csv",
            @"C:\dev\temp\temp\files\File3.csv"
        });

        var merged = MergeSortedChunks(chunkFiles);

        using var writer = new StreamWriter(@"C:\dev\temp\temp\files\Merged.csv");
        await foreach (var (Date, Value) in merged)
        {
            await writer.WriteLineAsync($"{Date},{Value}");
        }
        await writer.FlushAsync();
    }

    private static async Task<List<string>> CreateSortedChunksAsync(string[] filePaths)
    {
        var chunkFiles = new List<string>();

        foreach (var filePath in filePaths)
        {
            using var reader = new StreamReader(filePath);
            var lines = new List<(DateTime Date, int Value)>(ChunkSize);
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out DateTime parsedDate) && int.TryParse(parts[1], out int parsedValue))
                {
                    lines.Add((parsedDate, parsedValue));
                }

                if (lines.Count >= ChunkSize)
                {
                    chunkFiles.Add(await WriteChunkToFileAsync(lines));
                    lines.Clear();
                }
            }

            if (lines.Count > 0)
            {
                chunkFiles.Add(await WriteChunkToFileAsync(lines));
            }
        }

        return chunkFiles;
    }

    private static async Task<string> WriteChunkToFileAsync(List<(DateTime Date, int Value)> lines)
    {
        lines.Sort((x, y) => x.Date.CompareTo(y.Date));
        var chunkFilePath = Path.GetTempFileName();

        using var writer = new StreamWriter(chunkFilePath);
        foreach (var (Date, Value) in lines)
        {
            await writer.WriteLineAsync($"{Date},{Value}");
        }

        return chunkFilePath;
    }

    private static async IAsyncEnumerable<(DateTime Date, int Value)> MergeSortedChunks(List<string> chunkFiles)
    {
        var priorityQueue = new PriorityQueue<(DateTime Date, int Value, StreamReader Reader), DateTime>();
        var readers = chunkFiles.Select(file => new StreamReader(file)).ToList();

        try
        {
            foreach (var reader in readers)
            {
                if (await reader.ReadLineAsync() is string line)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && DateTime.TryParse(parts[0], out DateTime parsedDate) && int.TryParse(parts[1], out int parsedValue))
                    {
                        priorityQueue.Enqueue((parsedDate, parsedValue, reader), parsedDate);
                    }
                }
            }

            while (priorityQueue.Count > 0)
            {
                var (date, value, reader) = priorityQueue.Dequeue();
                yield return (date, value);

                if (await reader.ReadLineAsync() is string line)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && DateTime.TryParse(parts[0], out DateTime parsedDate) && int.TryParse(parts[1], out int parsedValue))
                    {
                        priorityQueue.Enqueue((parsedDate, parsedValue, reader), parsedDate);
                    }
                }
                else
                {
                    reader.Dispose();
                }
            }
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Dispose();
            }

            foreach (var chunkFile in chunkFiles)
            {
                File.Delete(chunkFile);
            }
        }
    }
}
