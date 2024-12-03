using System.Drawing;
using System.Windows.Forms;
using Flarial.Launcher;

sealed class Form : System.Windows.Forms.Form
{
    internal Catalog Catalog = default;

    internal Form()
    {
        Text = "Flarial Launcher";
        Font = SystemFonts.MessageBoxFont;
        ClientSize = new(800 / 2, 600 / 2);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        TabControl tabControl = new()
        {
            Dock = DockStyle.Fill,
            Visible = false
        };
        Controls.Add(tabControl);

        ProgressBar progressBar = new()
        {
            Height = 23,
            Style = ProgressBarStyle.Marquee,
            Anchor = AnchorStyles.None,
            MarqueeAnimationSpeed = 1
        };
        progressBar.Left = (ClientSize.Width - progressBar.Width) / 2;
        progressBar.Top = (ClientSize.Height - progressBar.Height) / 2;
        Controls.Add(progressBar);

        Load += async (_, _) =>
        {
            Catalog = await Catalog.GetAsync();
            progressBar.Visible = !(tabControl.Visible = true);
            tabControl.TabPages.AddRange([new Play(this), new Versions(this)]);
        };
    }
}