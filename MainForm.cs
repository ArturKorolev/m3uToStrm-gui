using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace m3uToStrm.Gui
{
    public class MainForm : Form
    {
        private Button btnChooseFile;
        private TextBox txtInputFile;
        private Button btnChooseFolder;
        private TextBox txtOutputFolder;
        private Button btnConvert;
        private TextBox txtLog;

        public MainForm()
        {
            Text = "m3uToStrm GUI";
            Width = 700;
            Height = 400;

            btnChooseFile = new Button { Text = "Выбрать m3u", Left = 10, Top = 10, Width = 120 };
            txtInputFile = new TextBox { Left = 140, Top = 12, Width = 520 };
            btnChooseFolder = new Button { Text = "Выбрать папку STRM", Left = 10, Top = 45, Width = 120 };
            txtOutputFolder = new TextBox { Left = 140, Top = 47, Width = 520 };
            btnConvert = new Button { Text = "Конвертировать", Left = 10, Top = 80, Width = 120 };
            txtLog = new TextBox { Left = 10, Top = 120, Width = 650, Height = 220, Multiline = true, ScrollBars = ScrollBars.Vertical };

            btnChooseFile.Click += BtnChooseFile_Click;
            btnChooseFolder.Click += BtnChooseFolder_Click;
            btnConvert.Click += BtnConvert_Click;

            Controls.Add(btnChooseFile);
            Controls.Add(txtInputFile);
            Controls.Add(btnChooseFolder);
            Controls.Add(txtOutputFolder);
            Controls.Add(btnConvert);
            Controls.Add(txtLog);
        }

        private void BtnChooseFile_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "M3U files|*.m3u|All files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtInputFile.Text = ofd.FileName;
            }
        }

        private void BtnChooseFolder_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtOutputFolder.Text = fbd.SelectedPath;
            }
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            txtLog.Clear();
            string input = txtInputFile.Text;
            string output = txtOutputFolder.Text;

            if (string.IsNullOrWhiteSpace(input) || !File.Exists(input))
            {
                MessageBox.Show("Выберите корректный .m3u файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(output) || !Directory.Exists(output))
            {
                MessageBox.Show("Выберите корректную папку для .strm", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Быстрый прототип: запуск существующего exe и захват вывода
            string exePath = Path.Combine(AppContext.BaseDirectory, "m3uToStrm.exe");
            if (!File.Exists(exePath))
            {
                AppendLog($"Не найден {exePath}. Будет выполнена внутренняя конвертация.");
                // Встроенная конверсия: читать m3u и записать strm в указанную папку
                await Conversion.ProcessM3UAsync(input, output, AppendLog);
                AppendLog("Готово (встроенный режим).");
                return;
            }

            AppendLog("Запуск внешнего процесса: " + exePath);
            var psi = new ProcessStartInfo(exePath, "")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                AppendLog("Не удалось запустить процесс.");
                return;
            }

            proc.OutputDataReceived += (s, ea) => { if (ea.Data != null) AppendLog(ea.Data); };
            proc.ErrorDataReceived += (s, ea) => { if (ea.Data != null) AppendLog("ERR: " + ea.Data); };
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();
            AppendLog($"Процесс завершился с кодом {proc.ExitCode}.");
        }

        private void AppendLog(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendLog(text)));
                return;
            }
            txtLog.AppendText(text + Environment.NewLine);
        }
    }
}
