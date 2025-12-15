namespace KSeF.DemoWebApp.Services;

public static class XadeSDummy
{
    public static async Task<string> SignWithPZ(string xml, string directoryPath, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromMinutes(5); // domyślnie 5 min
        TimeSpan pollInterval = TimeSpan.FromSeconds(1); // poll co sekundę

        // 1. Generuj unikalny guid
        string guid = Guid.NewGuid().ToString("N");
        string baseFileName = $"ksefDummySign_{guid}.xml";
        string fileToSignPath = Path.Combine(directoryPath, baseFileName);
        string fileSignedPath = Path.Combine(directoryPath, $"ksefDummySign_{guid} (1).xml");

        try
        {
            // 2. Zapisz oryginalny plik do podpisu
            await File.WriteAllTextAsync(fileToSignPath, xml).ConfigureAwait(false);

            Console.WriteLine($"File written: {fileToSignPath}. Waiting for signed version...");

            // 3. Czekaj aż pojawi się plik podpisany (ksefDummySign_{guid}(1).xml)
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                if (File.Exists(fileSignedPath))
                {
                    // #GreenFlag: plik podpisany jest, odczytujemy i zwracamy zawartość
                    return await File.ReadAllTextAsync(fileSignedPath).ConfigureAwait(false);
                }
                await Task.Delay(pollInterval).ConfigureAwait(false);
            }

            // #RedFlag: Timeout – plik się nie pojawił
            throw new TimeoutException($"Timeout: plik podpisany ({fileSignedPath}) nie pojawił się w ciągu {timeout.Value.TotalSeconds} sekund.");
        }
        catch (Exception ex)
        {
            // #PlanRollback: błąd – zwróć info
            return $"#RedFlag: Błąd – {ex.Message}";
        }
    }
}
