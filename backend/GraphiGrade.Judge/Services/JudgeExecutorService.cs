using GraphiGrade.Judge.Common;
using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.Interop;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Services.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace GraphiGrade.Judge.Services;

public class JudgeExecutorService : IJudgeExecutorService
{
    private readonly JudgeRunnerSettings _settings;
    private readonly ILogger<JudgeExecutorService> _logger;

    public JudgeExecutorService(
        IOptions<JudgeRunnerSettings> settings, 
        ILogger<JudgeExecutorService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<JudgeExecutorResult> ExecuteAsync(string outputProgramPath)
    {
        Process? process = null;

        try
        {
            InjectDllsInOutput(outputProgramPath);

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = outputProgramPath
                }
            };

            process.Start();

            await Task.Delay(_settings.MaximumRunningTimeMilliseconds);

            var mainHandle = process.MainWindowHandle;
            if (mainHandle == IntPtr.Zero)
            {
                return new JudgeExecutorResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Unable to capture window - mainWindowHandle was zero"
                };
            }

            // Capturing 
            WinAPI.GetClientRect(mainHandle, out WinAPI.RECT rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top - 20; // TEMP!

            ImageDecorator resultImage = new ImageDecorator(width, height, PixelFormat.Format32bppArgb);

            using (Graphics gfxBmp = resultImage.GraphicsFromImage())
            {
                var hdcBitmap = gfxBmp.GetHdc();
                bool success = WinAPI.PrintWindow(mainHandle, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);

                if (!success)
                {
                    return new JudgeExecutorResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Unable to capture window - PrintWindow failed"
                    };
                }
            }

            return new JudgeExecutorResult
            {
                IsSuccessful = true,
                ExecutionResult = resultImage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Error while executing user submission.");

            return new JudgeExecutorResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            process?.Kill();
        }
    }

    private void InjectDllsInOutput(string outputProgramLocation)
    {
        string? outputFolderLocation = Path.GetDirectoryName(outputProgramLocation);

        if (outputFolderLocation == null)
        {
            throw new Exception("Unable to inject DLLs - output folder was null");
        }

        string[] dlls = Directory.GetFiles(_settings.DllFolderPath, "*.dll");
        foreach (string dll in dlls)
        {
            File.Copy(dll, Path.Join(outputFolderLocation, Path.GetFileName(dll)), overwrite: true);
        }
    }
}
