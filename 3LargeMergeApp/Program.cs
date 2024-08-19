using _3LargeMergeApp;

class MainClass
{
    private static readonly Dictionary<int, int> fileProgress = new()
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 0 }
    };
    public static async Task Main()
    {
        await ReadAndMergeFiles.ReadAndMergeFilesAsync();
        //await RunCreateFiles();
    }
    

    private async static Task RunCreateFiles()
    {
        var progress = new Progress<(int fileIndex, int percent)>(report =>
        {
            UpdateProgress(report.fileIndex, report.percent);
        });

        Console.Clear();
        Console.WriteLine("File1 => 0% (Thread: 0)");
        Console.WriteLine("File2 => 0% (Thread: 0)");
        Console.WriteLine("File3 => 0% (Thread: 0)");

        await CreateFiles.GenerateFilesAsync(progress);
    }

    private static void UpdateProgress(int fileIndex, int percent)
    {
        fileProgress[fileIndex] = percent;
        int cursorTop = fileIndex;
        int threadId = Environment.CurrentManagedThreadId;
        Console.SetCursorPosition(0, cursorTop);
        Console.WriteLine($"File{fileIndex + 1} => {percent}% (Thread: {threadId})");
    }
}