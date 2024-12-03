using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Flarial.Launcher;
using Minecraft.UWP;

sealed class Play : TabPage
{
    internal Play(Form _)
    {
        Text = "Play";
        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.White
        };
        Controls.Add(panel);

        Button button = new()
        {
            Text = "Launch",
            Width = 150,
            Height = 50,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.None
        };

        CheckBox checkBox = new()
        {
            Text = "Beta",
            AutoSize = true,
            Checked = true,
            Anchor = AnchorStyles.None
        };

        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Fill,
            Height = 23,
            Visible = false,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 1
        };

        panel.RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.Controls.AddRange([button, checkBox, progressBar]);

        button.Click += async (_, _) =>
        {
            progressBar.Visible = !(button.Enabled = checkBox.Enabled = false);

            if (_.Catalog.Contains(await Game.VersionAsync()))
            {
                button.Text = "Launching...";
                
                await Client.DownloadAsync(checkBox.Checked, (_) =>
                {
                    if (progressBar.Value != _)
                    {
                        if (progressBar.Style is ProgressBarStyle.Marquee) progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = _;
                    }
                });

                progressBar.Value = 0; progressBar.Style = ProgressBarStyle.Marquee;
                await Client.LaunchAsync(checkBox.Checked);
            }

            progressBar.Visible = !(button.Enabled = checkBox.Enabled = true);
            button.Text = "Launch";
        };
    }
}