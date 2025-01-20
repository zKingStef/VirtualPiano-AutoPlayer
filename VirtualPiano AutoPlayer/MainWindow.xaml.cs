using Microsoft.Win32;
using System.IO;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;


namespace VirtualPiano_AutoPlayer
{
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
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task PlayMusicSheet(string sheet, CancellationToken token)
        {
            sheet = sheet.Replace("\r", "").Replace("\n", "").Trim();

            for (int i = 0; i < sheet.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                char command = sheet[i];

                await Task.Delay(200, token);

                if (command == ' ')
                {
                    await Task.Delay(300, token);
                }
                else if (command == '|')
                {
                    await Task.Delay(1000, token);
                }
                else if (command == '[')
                {
                    int endIndex = sheet.IndexOf(']', i);
                    if (endIndex != -1)
                    {
                        string simultaneousKeys = sheet.Substring(i + 1, endIndex - i - 1);
                        PressKeysSimultaneously(simultaneousKeys);
                        await Task.Delay(300, token);
                        i = endIndex;
                    }
                }
                else
                {
                    PressKey(command.ToString());
                    await Task.Delay(300, token);
                }
            }
        }

        private void PressKey(string key)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing key: {key}");

            simulator.Keyboard.TextEntry(key);
        }

        private void PressKeysSimultaneously(string keys)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing keys simultaneously: {keys}");

            foreach (char key in keys)
            {
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