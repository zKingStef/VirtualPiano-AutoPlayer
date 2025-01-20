using Microsoft.Win32;
using System.IO;
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
            string musicSheet = MusicSheetInput.Text;

            if (string.IsNullOrWhiteSpace(musicSheet))
            {
                StatusText.Text = "Status: Please enter a valid music sheet!";
                return;
            }

            StatusText.Text = "Status: Preparing to play...";

            // 3-second initial delay
            await Task.Delay(3000);

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
            foreach (char command in sheet)
            {
                token.ThrowIfCancellationRequested();

                if (command == ' ')
                {
                    // Short pause for space
                    await Task.Delay(400, token);
                }
                else if (command == '|')
                {
                    // 1-second pause for '|'
                    await Task.Delay(1000, token);
                }
                else if (command == '[')
                {
                    // Handle simultaneous key presses
                    int endIndex = sheet.IndexOf(']', sheet.IndexOf(command));
                    if (endIndex != -1)
                    {
                        string simultaneousKeys = sheet.Substring(sheet.IndexOf(command) + 1, endIndex - sheet.IndexOf(command) - 1);
                        PressKeysSimultaneously(simultaneousKeys);
                        await Task.Delay(400, token);
                    }
                }
                else
                {
                    // Regular key press
                    PressKey(command.ToString());
                    await Task.Delay(400, token);
                }
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

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MusicSheetInput.Text = File.ReadAllText(openFileDialog.FileName);

                    // Set the Title to the filename without extension
                    Title.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                    StatusText.Text = "Status: Music sheet loaded successfully!";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Status: Error loading file - {ex.Message}";
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Title.Text))
            {
                StatusText.Text = "Status: Please enter a title for the music sheet.";
                return;
            }

            string defaultFileName = Title.Text.Trim();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, MusicSheetInput.Text);
                    StatusText.Text = "Status: Music sheet saved successfully!";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Status: Error saving file - {ex.Message}";
                }
            }
        }
    }
}