using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace m3uToStrm.Gui
{
    internal static class Conversion
    {
        public static async Task ProcessM3UAsync(string inputFile, string outputFolder, Action<string> logger)
        {
            try
            {
                var lFiles = new List<string>();
                var lReference = new List<string>();

                using (var reader = new StreamReader(inputFile))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.Equals("#EXTM3U")) continue;
                        if (line.Length >= 10)
                        {
                            var sFirst = line.Substring(0, 10);
                            if (sFirst.Equals("#EXTVLCOPT")) continue;
                            if (sFirst.Equals("#EXTINF:0,"))
                            {
                                var fileName = line.Substring(10);
                                fileName = fileName.Replace(".mkv", ".strm").Replace(".avi", ".strm");
                                lFiles.Add(fileName);
                                continue;
                            }
                        }
                        lReference.Add(line);
                    }
                }

                if (lFiles.Count > 0)
                {
                    for (int i = 0; i < lFiles.Count; i++)
                    {
                        var outPath = Path.Combine(outputFolder, lFiles[i]);
                        logger($"Запись: {outPath}");
                        using (var sw = new StreamWriter(outPath, false))
                        {
                            await sw.WriteLineAsync(lReference[i]);
                        }
                    }
                }
                else
                {
                    logger("Не найдено записей для конвертации.");
                }
            }
            catch (Exception ex)
            {
                logger("Ошибка: " + ex.Message);
            }
        }
    }
}
