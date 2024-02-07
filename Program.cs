using Microsoft.VisualBasic;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DotNet.Docker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunE().Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
         
        }
        public async static Task Run()
        {
            var modelName = "ggml-large-v2.bin";
            if (!File.Exists(modelName))
            {
                Console.WriteLine($"{modelName} does not exist");
                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.LargeV2);
                using var fileWriter = File.OpenWrite(modelName);
                await modelStream.CopyToAsync(fileWriter);
            }
            using var whisperFactory = WhisperFactory.FromPath("ggml-large-v2.bin");

            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            int outRate = 16000;
            var inFile = @"Z:\Dl\louisa\louisa.wav";
            var outFile = @"Z:\\Dl\\louisa\\louisa16.wav";
            using (var reader = new WaveFileReader(inFile))
            {
                var outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    // resampler.ResamplerQuality = 60;
                    WaveFileWriter.CreateWaveFile(outFile, resampler);
                }
            }
            using var fileStream = File.OpenRead("Z:\\Dl\\louisa\\louisa16.wav");

            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
        }

        public async static Task RunE()
        {
            var ggmlType = GgmlType.LargeV3;
            var modelFileName = "Largesssst.bin";
            var wavFileName = "louisa16.wav";

            // This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
            if (!File.Exists(modelFileName))
            {
                await DownloadModel(modelFileName, ggmlType);
            }

            // This section creates the whisperFactory object which is used to create the processor object.
            using var whisperFactory = WhisperFactory.FromPath("Largesssst.bin");

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();


            using var fileStream = File.OpenRead(wavFileName);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
        }

        private static async Task DownloadModel(string fileName, GgmlType ggmlType)
        {
            Console.WriteLine($"Downloading Model {fileName}");
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            using var fileWriter = File.OpenWrite(fileName);

            var totalBytes = 3095033483L; // Taille totale du fichier
            var bufferSize = 81920; // 80 KB buffer size
            var buffer = new byte[bufferSize];
            var totalBytesRead = 0L;
            var bytesReadThisTime = 0;
            var lastProgressUpdate = 0;

            // Loop through the stream and copy it to the file
            while ((bytesReadThisTime = await modelStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileWriter.WriteAsync(buffer, 0, bytesReadThisTime);
                totalBytesRead += bytesReadThisTime;

                // Calculate the progress percentage
                var progressPercentage = (double)totalBytesRead / totalBytes * 100;

                // Display progress every 1% increase
                if ((int)progressPercentage > lastProgressUpdate)
                {
                    Console.WriteLine($"Download progress: {progressPercentage:F2}%");
                    lastProgressUpdate = (int)progressPercentage;
                }
            }

            Console.WriteLine($"Download completed: {totalBytesRead} bytes downloaded.");
        }




    }
}