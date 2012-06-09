using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Patcher
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool Patched { get; set; }
        private Dictionary<int, PatchList> LstPatchlist = new Dictionary<int,PatchList>();
        private static Config OConfig = new Config();
        private static int CurrentFile { get; set; }

        /// <summary>
        /// Konstruktor der Main Klasse
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Patched = false;
            CurrentFile = 0;
        }

        /// <summary>
        /// Diese Funktion wird ausgeführt, wenn das Fenster angeklickt wird
        /// </summary>
        private void WindowMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Diese Funktion beim Klick auf das Kreuz oben rechts ausgeführt
        /// </summary>
        private void ClickCloseBtn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Button Klick Funktion für "Spiel Starten"
        /// </summary>
        private void ClickPlayGameBtn(object sender, RoutedEventArgs e)
        {
            BtnStartGame.IsEnabled = false;
            LstPatchlist.Clear();

            if (Patched == true)
            {
                this.StartGame();
            }
            else
            {
                this.LoadPatchList();
            }
        }
        
        /// <summary>
        /// Funktion um Text zur Textbox hinzuzufügen
        /// </summary>
        /// <param name="txt"></param>
        private void AddTextToList(string txt)
        {
            PatchBox.Text += String.Format("{0}\r\n", txt);
            TextScroll.LineDown();
        }

        /// <summary>
        /// Funktion um Prozent des derzeitigen Downloadprogress zu verarbeiten
        /// </summary>
        private void EventDownloadProgres(object sender, DownloadProgressChangedEventArgs e)
        {
            LblPercentFile.Content = String.Format("{0}%", e.ProgressPercentage);
            ProgressFile.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Funktion zum Ermitteln des MD5-Hashs
        /// </summary>
        public string GetMD5FromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                return sb.ToString();
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// Funktion um die Patchlist in ein Dictionary zu schreiben
        /// </summary>
        private void PatchListToArray()
        {
            bool DirectoryPart = false;
            string Line;
            StreamReader FilePatchList = new StreamReader(OConfig.PatchList);

            this.AddTextToList("Patchlist wird gelesen...");

            int i = 0;

            while ((Line = FilePatchList.ReadLine()) != null)
            {
                if (Line == "[DIRECTORY]") {
                    DirectoryPart = true;
                    continue;
                }
                else if (Line == "[/DIRECTORY]")
                {
                    DirectoryPart = false;
                    continue;
                }

                if (DirectoryPart == true && !Directory.Exists(Line))
                {
                    Directory.CreateDirectory(Line);
                    this.AddTextToList(String.Format("{0} wurde angelegt.\r\n", GetShortName(Line)));
                }
                else if (DirectoryPart == false)
                {
                    PatchList LPList = new PatchList();
                    try
                    {
                        string[] ArrayLine = Line.Split('|');

                        LPList.FileName = ArrayLine[0];
                        LPList.HashValue = ArrayLine[1];

                        LstPatchlist.Add(i, LPList);
                    }
                    catch { }
                    LPList = null;
                    i++;
                }
            }

            FilePatchList.Close();
            File.Delete(OConfig.PatchList);
            this.AddTextToList("Patchlist wurde gelöscht.\r\n");

            this.RefreshInterface(0);
            this.CheckFiles(0);
        }

        /// <summary>
        /// Funktion um die Patchlist herunterzuladen
        /// </summary>
        private void LoadPatchList()
        {
            LblFileName.Content = "Datei: patchlist";
            this.AddTextToList("Patchlist wird heruntergeladen...");

            WebClient DLPatchList = new WebClient();
            DLPatchList.Proxy = null;
            DLPatchList.DownloadProgressChanged += new DownloadProgressChangedEventHandler(EventDownloadProgres);
            DLPatchList.DownloadFileCompleted += delegate
            {
                this.AddTextToList("Patchlist wurde heruntergeladen.");
                this.PatchListToArray();
            };
            DLPatchList.DownloadFileAsync(new Uri(String.Format("{0}patcher.php", OConfig.PatchURL)), OConfig.PatchList);
        }

        /// <summary>
        /// Funktion zum Aktualisieren der Oberfläche
        /// </summary>
        /// <param name="FileNr"></param>
        private void RefreshInterface(int FileNr)
        {
            LblTotalFiles.Content = String.Format("Gesamt: {0}/{1} Dateien", FileNr, LstPatchlist.Count.ToString());
            ProgressTotal.Maximum = LstPatchlist.Count;
        }

        /// <summary>
        /// Funktion zum Verkürzen des Dateinamen auf 20 Zeichen
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        private string GetShortName(string Filename)
        {
            string ShortName = String.Empty;
            if (Filename.Length > 20)
            {
                ShortName = Filename.Substring(0, Math.Min(20, Filename.Length)) + "...";
            }
            else
            {
                ShortName = Filename;
            }

            return ShortName;
        }

        /// <summary>
        /// Funktion zum Aktualisieren der Currentelements auf der GUI
        /// </summary>
        /// <param name="Filename"></param>
        private void RefreshFileInterface(string Filename)
        {
            LblFileName.Content = String.Format("Datei: {0}", Filename);
            LblPercentFile.Content = "0%";
            ProgressFile.Value = 0;
        }

        /// <summary>
        /// Funktion zum Aktualisieren der Currentelements auf der GUI
        /// </summary>
        /// <param name="FileNumber"></param>
        public void RefreshCurrentInterface(int FileNumber)
        {
            LblTotalFiles.Content = String.Format("Gesamt: {0}/{1} Dateien", FileNumber, LstPatchlist.Count.ToString());
            LblTotalPercent.Content = String.Format("{0}%", (FileNumber * 100 / LstPatchlist.Count));
            ProgressTotal.Value = FileNumber;
            CheckFiles(FileNumber);
        }

        /// <summary>
        /// Funktion Abarbeiten der Dateien
        /// </summary>
        /// <param name="FileNumber"></param>
        private void CheckFiles(int FileNumber)
        {
            if (LstPatchlist.Count > FileNumber)
            {
                string Shortname = GetShortName(LstPatchlist[FileNumber].FileName);
                this.AddTextToList(String.Format("{0} wird geprüft...", Shortname));

                if (LstPatchlist[FileNumber].HashValue != this.GetMD5FromFile(LstPatchlist[FileNumber].FileName))
                {
                    this.AddTextToList(String.Format("{0} wird heruntergeladen...", Shortname));

                    RefreshFileInterface(Shortname);

                    WebClient FileDownload = new WebClient();
                    FileDownload.Proxy = null;
                    FileDownload.DownloadProgressChanged += new DownloadProgressChangedEventHandler(EventDownloadProgres);
                    FileDownload.DownloadFileCompleted += delegate
                    {
                        this.AddTextToList(String.Format("{0} wurde heruntergeladen.\r\n", Shortname));
                        RefreshCurrentInterface(FileNumber + 1);
                    };
                    FileDownload.DownloadFileAsync(new Uri(String.Format("{0}client/{1}", OConfig.PatchURL, LstPatchlist[FileNumber].FileName)), LstPatchlist[FileNumber].FileName);
                }
                else
                {
                    this.AddTextToList(String.Format("{0} ist aktuell.\r\n", Shortname));
                    RefreshCurrentInterface(FileNumber + 1);
                }
            }
            else
            {
                this.StartGame();
            }
        }

        /// <summary>
        /// Funktion zum Starten des Spiels
        /// </summary>
        private void StartGame()
        {
            this.AddTextToList("Das Spiel wird gestartet.");

            Process.Start("cmd", String.Format("/c start {0}", OConfig.GameBinary));
            BtnStartGame.IsEnabled = true;
        }

        /// <summary>
        /// Funktion beim Klicken auf Homapge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickHomepageBtn(object sender, RoutedEventArgs e)
        {
            Process.Start(OConfig.HomepageURL);
        }

        /// <summary>
        /// Funktion beim Klicken auf Einstellungen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickSettingsBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("config.exe");
            }
            catch
            {
                MessageBox.Show("Die config.exe konnte nicht gefunden werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
