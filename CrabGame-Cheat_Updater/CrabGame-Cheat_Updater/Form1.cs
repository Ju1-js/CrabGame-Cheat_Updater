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
        string releaseVersion;
        string localVersion;
        readonly string downloadLocation = Properties.Settings.Default.downloadLocation;

        public Form1()
        {
            localVersion = Properties.Settings.Default.localVersion;

            InitializeComponent();
            LatestRelease();
            CompareVersions();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadPrompt();
        }

        private void DownloadPrompt()
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
            localVersion = LatestRelease();
            Properties.Settings.Default.localVersion = localVersion;
            Properties.Settings.Default.Save();

            MessageBox.Show("Download completed!\nSaved to: " + path, "Download Complete");
            Process.Start(new ProcessStartInfo("explorer.exe", path));
        }

        private string LatestRelease()
        {
            string releasePath = "https://github.com/DasJNNJ/CrabGame-Cheat/releases/latest";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(releasePath);
            request.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string redirUrl = response.Headers["Location"];
            response.Close();
            Uri uri = new Uri(redirUrl);
            releaseVersion = uri.AbsolutePath.Replace("/DasJNNJ/CrabGame-Cheat/releases/tag/", "");

            Console.WriteLine(releaseVersion);
            return releaseVersion;
        }

        private void CompareVersions()
        {
            List<string> localversion = new List<string>(localVersion.Split('.'));
            List<string> releaseversion = new List<string>(releaseVersion.Split('.'));

            Console.WriteLine($"{localversion} || {releaseversion}");

            if (localversion.Count < releaseversion.Count)
            {
                while (localversion.Count < releaseversion.Count)
                {
                    localversion.Add("0");
                    Console.WriteLine("Extra values (0) added to localversion list");
                }
            }
            else if (localversion.Count > releaseversion.Count)
            {
                while (localversion.Count > releaseversion.Count)
                {
                    releaseversion.Add("0");
                    Console.WriteLine("Extra values (0) added to releaseversion list");
                }
            }

            for (int i = 0; i < localversion.Count; i++)
            {
                if (int.Parse(localversion[i]) < int.Parse(releaseversion[i]))
                {
                    //Console.WriteLine("Local release is behind");
                    DialogResult dialogResult = MessageBox.Show("Local release is behind, would you like to update?", "New Update Found", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        DownloadPrompt();
                        break;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        break;
                    }
                }
                else if (int.Parse(localversion[i]) > int.Parse(releaseversion[i]))
                {
                    //Console.WriteLine("Local release is ahead?!");
                    MessageBox.Show("Local release is ahead, this shoulden't be possible...");
                    break;
                }
                else if (int.Parse(localversion[i]) == int.Parse(releaseversion[i]))
                {
                    //Console.WriteLine("Local release is up to date");
                    MessageBox.Show("Local release is up to date.");
                    break;
                }
                else
                {
                    //Console.WriteLine("Error checking version");
                    MessageBox.Show("Error checking version.");
                    break;
                }
            }
        }
    }
}
