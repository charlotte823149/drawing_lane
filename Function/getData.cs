using System.Windows.Forms;

namespace drawing_lane.Function
{
    class getData
    {
        public string path { get; set; }
        public string SelectedPath()
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            return path.SelectedPath;
        }

        public string SelectedFile()
        {
            OpenFileDialog file = new OpenFileDialog();

            //filer files
            file.DefaultExt = ".txt";
            file.Filter = "Text documents (.txt)|*.txt";

            //show open file dialog box
            file.ShowDialog();            
            return file.FileName;
        }        
    }
}
