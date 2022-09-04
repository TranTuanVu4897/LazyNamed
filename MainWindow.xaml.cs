using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using sysPath = System.IO.Path;

namespace LazyNamed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<FileEntry> fileEntries;

        public MainWindow()
        {
            fileEntries = new ObservableCollection<FileEntry>();
            InitializeComponent();
            lvResult.ItemsSource = fileEntries;
        }


        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            RegexOptions option = cbxCaseSensitive.IsChecked == true ? RegexOptions.IgnoreCase : RegexOptions.None;
            for (int i = 0; i < fileEntries.Count; i++)
            {
                FileEntry file = fileEntries[i];
                if (cbxUseRegex.IsChecked == true)
                {
                    file.ReplaceNameWithRegex(tbxFind.Text, tbxReplace.Text, option);
                }
                else
                {
                    file.ReplaceName(tbxFind.Text, tbxReplace.Text);
                }
            }
            this.ShowAcceptButton();
        }


        private void GetAllFileFromPath(string path)
        {
            if (Directory.Exists(path))
            {
                AddFilesToFileEntries(Directory.GetFiles(path));
                GetAllFileFromSubPath(Directory.GetDirectories(path));
            }
        }

        private void AddFilesToFileEntries(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                fileEntries.Add(FileEntry.initialFromFilePath(files[i]));
            }
        }

        private void GetAllFileFromSubPath(string[] subPaths)
        {
            for (int i = 0; i < subPaths.Length; i++)
            {
                GetAllFileFromPath(subPaths[i]);
            }
        }

        private void ClearCache()
        {
            fileEntries.Clear();
            btnAccept.IsEnabled = false;
            lblInfo.Visibility = Visibility.Hidden;
        }

        private void ShowAcceptButton()
        {
            btnAccept.IsEnabled = true;
            lblInfo.Visibility = Visibility.Visible;
        }

        private void tbxPath_LostFocus(object sender, RoutedEventArgs e)
        {
            var path = (sender as TextBox).Text;
            if (string.IsNullOrEmpty(path))
                return;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Path not valid.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            GetAllFileToCache(path);
        }

        private void GetAllFileToCache(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = tbxPath.Text;
            }

            if (!Directory.Exists(path))
            {
                return;
            }
            ClearCache();
            GetAllFileFromPath(path);
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < fileEntries.Count; i++)
            {
                fileEntries[i].MoveFile();
            }
            GetAllFileToCache();
            MessageBox.Show("Done.", "Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class FileEntry : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Extension { get; set; }
        private bool isUpdateName;

        public bool IsUpdateName
        {
            get { return isUpdateName; }
            set
            {
                isUpdateName = value;
                RaisePropertyChanged(nameof(IsUpdateName));
            }
        }

        private string newName;

        public string NewName
        {
            get { return newName; }
            set
            {
                newName = value;
                if (!string.IsNullOrEmpty(newName)) { RaisePropertyChanged(nameof(NewName)); }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public static FileEntry initialFromFilePath(string filePath)
        {
            return new FileEntry()
            {
                Name = sysPath.GetFileNameWithoutExtension(filePath),
                FilePath = sysPath.GetDirectoryName(filePath),
                Extension = sysPath.GetExtension(filePath),
                NewName = string.Empty,
                IsUpdateName = false
            };
        }

        public void ReplaceName(string find, string replace)
        {
            NewName = Name.Replace(find, replace);
            IsUpdateName = true;
        }

        public void ReplaceNameWithRegex(string find, string replace, RegexOptions options)
        {
            this.NewName = Regex.Replace(Name, find, replace, options);
            IsUpdateName = true;
        }

        public string GetFullPath(string name)
        {
            return string.Format("{0}/{1}{2}", FilePath, name, Extension);
        }

        public void MoveFile()
        {
            File.Move(GetFullPath(Name), GetFullPath(NewName));
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
