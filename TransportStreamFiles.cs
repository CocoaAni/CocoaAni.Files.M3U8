namespace CocoaAni.Files.M3U8;

public class TransportStreamFiles
{
    public string MainFilePath { get; set; }

    public TransportStreamFiles(string mainFilePath)
    {
        Paths = new List<string>();
        MainFilePath = mainFilePath;
    }

    public List<string> Paths { get; set; }

    public Task MergeAsync(Func<Memory<byte>, Memory<byte>>? filter = null, Action<int>? itemMergedEvent = null)
    {
        if (File.Exists(MainFilePath))
        {
            File.Delete(MainFilePath);
        }

        var fileStream = File.OpenWrite(MainFilePath);
        return MergeAsync(fileStream, filter, itemMergedEvent);
    }

    public Task MergeAsync(string savePath, Func<Memory<byte>, Memory<byte>>? filter = null, Action<int>? itemMergedEvent = null)
    {
        MainFilePath = savePath;
        return MergeAsync(filter, itemMergedEvent);
    }

    public async Task MergeAsync(Stream outputStream, Func<Memory<byte>, Memory<byte>>? filter = null, Action<int>? itemMergedEvent = null)
    {
        byte[]? buffer = null;
        for (var index = 0; index < Paths.Count; index++)
        {
            try
            {
                var path = Paths[index];
                var fs = File.OpenRead(path);
                if (buffer == null || buffer.Length != fs.Length)
                {
                    buffer = new byte[fs.Length];
                }

                var readAllLen = 0;
                while (readAllLen < fs.Length)
                {
                    var readLen = await fs.ReadAsync(buffer.AsMemory(readAllLen, buffer.Length - readAllLen));
                    readAllLen += readLen;
                }
                fs.Close();
                var memory = filter?.Invoke(buffer) ?? buffer;
                await outputStream.WriteAsync(memory);
                await outputStream.FlushAsync();
                itemMergedEvent?.Invoke(index + 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        if (outputStream is FileStream)
        {
            outputStream.Close();
        }
    }
}