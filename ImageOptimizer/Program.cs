using ImageOptimizer.Models;
using ImageOptimizer.Models.Enums;
using ImageOptimizer.Services;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;

namespace ImageOptimizer
{
    /// <summary>
    /// Entry point of the application. Orchestrates user input, mode detection,
    /// service initialization, benchmark execution, and result rendering.
    /// </summary>
    class Program
    {
        #region Fields

        private const string JsonExtension = ".json";

        #endregion

        #region Main

        /// <summary>
        /// Application entry point. Runs the full benchmark workflow.
        /// </summary>
        /// <param name="args">Command-line arguments (currently unused).</param>
        /// <returns>0 on success, 1 on invalid input.</returns>
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ShowBanner();

            // --- Inputs ---
            var inputPath = AskPath(
                prompt: "[bold cyan]Input path[/] [grey](image folder for MVP, or .json file for V1)[/]",
                mustExist: true
            );

            var outputDir = AskPath(
                prompt: "[bold cyan]Output directory[/]",
                mustExist: false
            );

            // --- Mode detection ---
            if (!TryDetectMode(inputPath, out var mode))
            {
                return 1;
            }

            AnsiConsole.Clear();
            ShowBanner();
            ShowConfigPanel(inputPath, outputDir, mode);

            // -- Service initialization ---
            IImageProcessorService sequentialImageProcessorService;
            IImageProcessorService parallelImageProcessorService;
            HttpClient? httpClient = null;

            if (mode == ModeEnum.FOLDER)
            {
                sequentialImageProcessorService = new SequentialImageProcessor();
                parallelImageProcessorService = new ParallelImageProcessor();

            }
            else
            {
                httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ImageOptimizer/1.0");
                var downloader = new ImageDownloaderService(httpClient);

                sequentialImageProcessorService = new SequentialImageProcessor(downloader);
                parallelImageProcessorService = new ParallelImageProcessor(downloader);
            }

            // --- Execution with progress animation ---
            try
            {
                TimeSpan sequentialTime = TimeSpan.Zero, parallelTime = TimeSpan.Zero;

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync("Preparing...", async ctx =>
                    {
                        if (mode == ModeEnum.FOLDER)
                        {
                            ctx.Status("[yellow]Sequential processing...[/]");
                            sequentialTime = await Benchmark(
                                () => sequentialImageProcessorService.ProcessLocalFolderAsync(inputPath, Path.Combine(outputDir, "SequentialImages")));

                            ctx.Status("[green]Parallel processing...[/]");
                            parallelTime = await Benchmark(
                                () => parallelImageProcessorService.ProcessLocalFolderAsync(inputPath, Path.Combine(outputDir, "ParallelImages")));
                        }
                        else
                        {
                            ctx.Status("[yellow]Loading sources...[/]");
                            var sources = await LoadSourcesAsync(inputPath);
                            AnsiConsole.MarkupLine($"[grey]{sources.Count} URL(s) loaded[/]");

                            ctx.Status("[yellow]Sequential processing...[/]");
                            sequentialTime = await Benchmark(
                                () => sequentialImageProcessorService.ProcessUrlsAsync(sources, Path.Combine(outputDir, "SequentialImages")));

                            ctx.Status("[green]Parallel processing...[/]");
                            parallelTime = await Benchmark(
                                () => parallelImageProcessorService.ProcessUrlsAsync(sources, Path.Combine(outputDir, "ParallelImages")));
                        }
                    });

                ShowResults(sequentialTime, parallelTime);
            }
            finally
            {
                httpClient?.Dispose();
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to exit...[/]");
            Console.ReadKey();
            return 0;
        }

        #endregion

        #region UI

        /// <summary>
        /// Displays the application banner with the project name and a separator.
        /// </summary>
        private static void ShowBanner()
        {
            AnsiConsole.Write(
                new FigletText("ImageOptimizer")
                    .Centered()
                    .Color(Color.Cyan1));

            AnsiConsole.WriteLine();

            AnsiConsole.Write(
                new Rule()
                    .RuleStyle("grey")
                    .Centered());

            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Displays a summary panel showing the selected mode and resolved input/output paths.
        /// </summary>
        /// <param name="inputPath">Path provided by the user as input source.</param>
        /// <param name="outputDir">Folder where generated WebP files will be written.</param>
        /// <param name="mode">Detected execution mode (FOLDER or FILE).</param>
        private static void ShowConfigPanel(string inputPath, string outputDir, ModeEnum mode)
        {
            var modeLabel = mode == ModeEnum.FOLDER
                ? "[green]FOLDER[/] [grey](local folder)[/]"
                : "[blue]FILE[/] [grey](remote URLs)[/]";

            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(2))
                .AddColumn()
                .AddRow("[bold]Mode[/]", modeLabel)
                .AddRow("[bold]Input[/]", $"[grey]{Path.GetFullPath(inputPath).EscapeMarkup()}[/]")
                .AddRow("[bold]Output[/]", $"[grey]{Path.GetFullPath(outputDir).EscapeMarkup()}[/]");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("[bold cyan] Configuration [/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("cyan")));

            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Displays the benchmark results as a table with sequential time, parallel time, and speedup.
        /// </summary>
        /// <param name="seqTime">Elapsed time for the sequential run.</param>
        /// <param name="parTime">Elapsed time for the parallel run.</param>
        private static void ShowResults(TimeSpan seqTime, TimeSpan parTime)
        {
            var gain = seqTime.TotalMilliseconds / parTime.TotalMilliseconds;

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .AddColumn(new TableColumn("[bold]Method[/]").Centered())
                .AddColumn(new TableColumn("[bold]Duration[/]").RightAligned());

            table.AddRow("Sequential", $"[yellow]{seqTime.TotalSeconds:F2}s[/]");
            table.AddRow("Parallel", $"[green]{parTime.TotalSeconds:F2}s[/]");
            table.AddRow("[bold]Speedup[/]", $"[bold cyan]x{gain:F2}[/]");

            AnsiConsole.WriteLine();

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();

            AnsiConsole.Write(
                new Rule()
                    .RuleStyle("grey")
                    .Centered());
        }

        #endregion

        #region User Input

        /// <summary>
        /// Prompts the user for a path and validates it. Loops until a valid path is entered.
        /// </summary>
        /// <param name="prompt">Markup-formatted prompt shown to the user.</param>
        /// <param name="mustExist">If true, the path must exist on disk (file or folder).</param>
        /// <returns>The trimmed path entered by the user.</returns>
        private static string AskPath(string prompt, bool mustExist)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>(prompt + " :")?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(input))
                {
                    AnsiConsole.MarkupLine("[red]Path cannot be empty.[/]");
                    continue;
                }

                if (mustExist && !Directory.Exists(input) && !File.Exists(input))
                {
                    AnsiConsole.MarkupLine($"[red]Path '[white]{input.EscapeMarkup()}[/]' does not exist.[/]");
                    continue;
                }

                return input;
            }
        }

        #endregion

        #region Mode Detection

        /// <summary>
        /// Detects the execution mode from the input path: FOLDER if it's a directory,
        /// FILE if it's a .json file. Returns false on unsupported input.
        /// </summary>
        /// <param name="inputPath">Path to inspect.</param>
        /// <param name="mode">Resolved mode when the method returns true.</param>
        /// <returns>True if a mode was detected, false otherwise.</returns>
        private static bool TryDetectMode(string inputPath, out ModeEnum mode)
        {
            if (Directory.Exists(inputPath))
            {
                mode = ModeEnum.FOLDER;
                return true;
            }

            var ext = Path.GetExtension(inputPath).ToLowerInvariant();
            if (ext == JsonExtension)
            {
                mode = ModeEnum.FILE;
                return true;
            }

            AnsiConsole.Clear();
            ShowBanner();
            AnsiConsole.MarkupLine($"[red]Unsupported extension: [white]{ext}[/] (expected: .json)[/]");
            mode = default;
            return false;
        }

        #endregion

        #region Sources Loading

        /// <summary>
        /// Loads the list of image sources from a JSON file.
        /// </summary>
        /// <param name="path">Path to the JSON file containing the sources array.</param>
        /// <returns>The deserialized list of image sources.</returns>
        private static async Task<List<ImageModel>> LoadSourcesAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<ImageModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        #endregion

        #region Benchmark

        /// <summary>
        /// Measures the elapsed time of an asynchronous action.
        /// </summary>
        /// <param name="action">Action to execute and time.</param>
        /// <returns>The elapsed time of the action.</returns>
        private static async Task<TimeSpan> Benchmark(Func<Task> action)
        {
            var sw = Stopwatch.StartNew();
            await action();
            sw.Stop();
            return sw.Elapsed;
        }

        #endregion
    }
}
