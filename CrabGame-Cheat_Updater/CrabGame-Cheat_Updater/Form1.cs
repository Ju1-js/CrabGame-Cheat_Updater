using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace CrabGame_Cheat_Updater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string downloadLocation = Properties.Settings.Default.downloadLocation;

        private void btnDownload_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog(); // sfd for saveFileDialouge
            sfd.InitialDirectory = Convert.ToString("C:/Program Files (x86)/Steam/steamapps/common/Crab Game/BepInEx/plugins");
            sfd.Filter = "Application Extension (*.dll)|*.dll|All Files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.OverwritePrompt = true;
            sfd.ValidateNames = true;
            WebClient webClient = new WebClient();

            var data = webClient.DownloadData(downloadLocation);
            string fileName;
            // Try to extract the filename from the Content-Disposition header
            if (!String.IsNullOrEmpty(webClient.ResponseHeaders["Content-Disposition"]))
            {
                fileName = webClient.ResponseHeaders["Content-Disposition"].Substring(webClient.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                sfd.FileName = fileName;
            }
            else
            {
                sfd.FileName = "CrabGame_Cheat_BepInEx.dll";
            }
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                SetPath(sfd.FileName);
                webClient.DownloadFileAsync(new Uri(downloadLocation), sfd.FileName);
            }
        }

        public string path;
        private void SetPath(string pathName)
        {
            path = Path.GetDirectoryName(pathName);
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download completed!\nSaved to: " + path, "Download Complete");
            Process.Start(new ProcessStartInfo("explorer.exe", path));
        }
    }
}
