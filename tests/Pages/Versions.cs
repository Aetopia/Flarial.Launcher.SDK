using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flarial.Launcher;

sealed class Versions : TabPage
{
    internal Versions(Form _)
    {
        Text = "Versions";
        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White,
            Enabled = false
        };
        Controls.Add(panel);

        ListBox listBox = new() { Dock = DockStyle.Fill };

        Button button1 = new()
        {
            Text = "Install",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Height = 23
        };

        Button button2 = new()
        {
            Text = "Cancel",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Height = 23,
            Visible = false
        };

        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Fill,
            Height = 23,
            Visible = false,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 1
        };

        Request request = default;

        Application.ThreadExit += (_, _) => { request?.Cancel(); };

        listBox.VisibleChanged += async (_, _) =>
        {
            await Task.Run(() => { foreach (var item in _.Catalog.Reverse()) listBox.Items.Add(item); });
            listBox.SelectedIndex = 0;
            panel.Enabled = true;
        };

        button1.Click += async (_, _) =>
        {
            progressBar.Visible = !(button1.Visible = listBox.Enabled = false);

            request = await _.Catalog.InstallAsync((string)listBox.SelectedItem, (_) =>
            {
                if (progressBar.Value != _)
                {
                    if (progressBar.Style is ProgressBarStyle.Marquee) progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = _;
                }
            });
            button2.Visible = true;
            try { await request; } catch (OperationCanceledException) { }


            button2.Visible = progressBar.Visible = !(button2.Enabled = button1.Visible = listBox.Enabled = true);
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Marquee;
        };

        button2.Click += async (_, _) =>
        {
            button2.Enabled = false;
            await request.CancelAsync();
        };

        panel.RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.Controls.AddRange([listBox, button1, button2, progressBar]);
    }
}