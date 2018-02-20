using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime;//usei para teste de conexao com internet
using System.Runtime.InteropServices;//usei para teste de conexao com internet
using YoutubeExtractor;
using System.Media;

namespace YoutubeBR
{
    public partial class YoutubeBR : Form
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        float valorBarraProgresso = 0;
        string tituloVideo;
        public YoutubeBR()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboResolucao.SelectedIndex = 0;
            pictureBox1.Visible = false;
        }

        public static bool IsConnectedToInternet()//verifica conexão com a internet
        {

            int Desc;
            return InternetGetConnectedState(out Desc, 0);

        }

        public bool Conexao()//verifica conexão com a internet
        {
            return IsConnectedToInternet();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (IsConnectedToInternet())
            {
                if (!(txtUrl.Text.Equals("") || txtDiretorio.Text.Equals("")))
                {
                    pictureBox1.Visible = true;
                    try
                    {
                        button4.Enabled = false;
                        //listaUrl.[i];
                        progressBar.Minimum = 0;
                        progressBar.Maximum = 100;

                        IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(txtUrl.Text);
                        VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == Convert.ToInt32(cboResolucao.Text));
                        if (video.RequiresDecryption)
                            DownloadUrlResolver.DecryptDownloadUrl(video);
                        VideoDownloader download = new VideoDownloader(video, Path.Combine(txtDiretorio.Text, video.Title + video.VideoExtension));
                        download.DownloadProgressChanged += Downloder_DownloadProgressChanged;
                        tituloVideo = video.Title;
                        Thread thread = new Thread(() => { download.Execute(); }) { IsBackground = true };
                        thread.Start();
                        //i++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro: " + ex);
                        button4.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Campo URL ou LOCAL vazio.");
                }
            }
            else
            {
                MessageBox.Show("Erro ao tentar conectar com a internet.");
            }
        }

        private void Downloder_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                progressBar.Value = (int)e.ProgressPercentage;
                lblPercentage.Text = $"{String.Format("{0:0.##}", e.ProgressPercentage)}%";
                progressBar.Update();
                if (progressBar.Value == 100)
                {
                    SystemSounds.Beep.Play();
                    notificacao();
                    //MessageBox.Show("Download Completo.");
                    progressBar.Value = 0;
                    txtUrl.Text = "";
                    button4.Enabled = true;

                }
                if (progressBar.Value > 0)
                {
                    pictureBox1.Visible = false;
                    valorBarraProgresso = progressBar.Value;//usando essa variavel para setar a mensagem ao minimizar atela
                }
            }));


        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Selecionar Diretório" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDiretorio.Text = fbd.SelectedPath;
                }
            }
        }

        public void notificacao()
        {
            notifyIcon1.ShowBalloonTip(2000, "YoutubeBR", "Download do Vídeo: " + tituloVideo + "Concluído com Sucesso", ToolTipIcon.Info);
            //execultaSomNotificacao();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                if (valorBarraProgresso > 0)
                    notifyIcon1.ShowBalloonTip(1000, "YoutubeBR", "Você tem Downloads em Andamento", ToolTipIcon.Info);
                else
                    notifyIcon1.ShowBalloonTip(1000, "YoutubeBR", " ", ToolTipIcon.Info);
            }
        }

        private void sairToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (valorBarraProgresso > 0)
            {
                var dialogResult = MessageBox.Show("Downloads em andamento deseja realmente sair da aplicação?", "Rebanho 1.0.0", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }else
            {
                Application.Exit();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.GetText().Equals(""))
                {
                    txtUrl.Text = Clipboard.GetText();
                }
            }catch(Exception ex)
            {
                throw new Exception("Erro - " + ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (valorBarraProgresso > 0)
            {
                var dialogResult = MessageBox.Show("Downloads em andamento deseja realmente sair da aplicação?", "Rebanho 1.0.0", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }else
            {
                Application.Exit();
            }
        }
    }
}
