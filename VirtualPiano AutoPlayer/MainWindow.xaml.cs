using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;


namespace VirtualPiano_AutoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Hole das Music Sheet aus der TextBox
            string musicSheet = MusicSheetInput.Text;

            if (string.IsNullOrWhiteSpace(musicSheet))
            {
                StatusText.Text = "Status: Please enter a valid music sheet!";
                return;
            }

            // Starte das Abspielen
            StatusText.Text = "Status: Playing...";
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await PlayMusicSheet(musicSheet, _cancellationTokenSource.Token);
                StatusText.Text = "Status: Finished playing!";
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Status: Playback stopped.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Status: Error - {ex.Message}";
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Stoppe das Abspielen
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task PlayMusicSheet(string sheet, CancellationToken token)
        {
            // Zerlege das Music Sheet in einzelne Kommandos
            string[] commands = sheet.Split(' ');

            foreach (string command in commands)
            {
                token.ThrowIfCancellationRequested();

                if (command.StartsWith("[") && command.EndsWith("]"))
                {
                    // Simultane Tastenanschläge, z. B. [fh]
                    string simultaneousKeys = command.Trim('[', ']');
                    PressKeysSimultaneously(simultaneousKeys);
                }
                else
                {
                    // Einzelner Tastenanschlag, z. B. a, p
                    PressKey(command);
                }

                // Warte zwischen den Noten (300ms als Standard)
                await Task.Delay(300, token);
            }
        }

        private void PressKey(string key)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing key: {key}");

            // Simuliere den Tastendruck
            simulator.Keyboard.TextEntry(key);
        }

        private void PressKeysSimultaneously(string keys)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing keys simultaneously: {keys}");

            foreach (char key in keys)
            {
                // Simuliere jeden Tastenanschlag
                simulator.Keyboard.TextEntry(key.ToString());
            }
        }
    }
}