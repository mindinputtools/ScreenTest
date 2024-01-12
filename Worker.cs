using DavyKager;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ScreenTest;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private static int _x, _y, _w, _h;
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        var screen = SystemInformation.VirtualScreen;
        _x = screen.Left;
        _y = screen.Top;
        _w = screen.Width;
        _h = screen.Height;

    }

    private void InitScreenReaderSupport()
    {
        Tolk.Load();
        string name = Tolk.DetectScreenReader();
        Tolk.Output($"Using {name}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var id = HotKeyManager.RegisterHotKey(Keys.PrintScreen, KeyModifiers.Alt);
        var id2 = HotKeyManager.RegisterHotKey(Keys.PrintScreen, KeyModifiers.Shift);
        HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
        InitScreenReaderSupport();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                //                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
        HotKeyManager.UnregisterHotKey(id2);
        HotKeyManager.UnregisterHotKey(id);
        Tolk.Unload();
    }
    static void HotKeyPressed(object sender, HotKeyEventArgs e)
    {
        switch (e.Modifiers)
        {
            case KeyModifiers.Alt:
                SaveImage();
                Tolk.Output("Captured image");
                break;
            case KeyModifiers.Shift:
                Thread t = new Thread(new ThreadStart(async () =>
                {
                    if (Clipboard.ContainsImage())
                    {
                        var data = Clipboard.GetDataObject();
                        var types = data.GetFormats(false);
                        MemoryStream stream = null;
                        if (types.Contains("PNG"))
                        {
                            stream = Clipboard.GetData("PNG") as MemoryStream;
                        }
                        else if (types.Contains("JPG"))
                        {
                            stream = Clipboard.GetData("JPG") as MemoryStream;
                        }
                        else if (types.Contains("Bitmap"))
                        {
                            stream = new MemoryStream();
                            Clipboard.GetImage().Save(stream, ImageFormat.Png);
                        }
                        if (stream != null)
                        {
                            Tolk.Output("Sending  clipboardimage");
                            var r = await OLLamaApi.GetResponse(stream.ToArray());
                            Tolk.Output(r);
                        }
                        else
                        {
                            Tolk.Output("Error getting image from clipboard");
                        }

                    }
                    else
                        Tolk.Output("No image on clipboard");

                }));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                break;
            default:
                break;
        }
    }

    private static async void SaveImage()
    {
        // string picturesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        // string fileName = $"Screenshot {DateTime.Now:yyyy-MM-dd HHmmss}.png";
        // string filePath = Path.Combine(picturesDirectory, fileName);
        //var image = new Bitmap(_w, _h);
        //using var graphics = Graphics.FromImage(image);
        //graphics.CopyFromScreen(_x, _y, 0, 0, new Size(_w, _h));
        var image = ScreenCapture.CaptureActiveWindow();
        using var s = new MemoryStream();
        image.Save(s, ImageFormat.Png);
        //        Thread t = new Thread(new ThreadStart(() => { Clipboard.SetImage(image); }));
        //        t.SetApartmentState(ApartmentState.STA);
        //        t.Start();
        var r = await OLLamaApi.GetResponse(s.ToArray());
        Tolk.Output(r);
    }
}
