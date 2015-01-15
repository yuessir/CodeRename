using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Rename
{
    public partial class Form1 : Form
    {
        private string strSearch;
        private string strReplace;
        private int count;

        public Form1()
        {
            InitializeComponent();
        }
        #region CheckIsTextFile
        /// <summary>
        /// Checks the file is textfile or not.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static bool CheckIsTextFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            bool isTextFile = true;
            try
            {
                int i = 0;
                int length = (int)fs.Length;
                byte data;
                while (i < length && isTextFile)
                {
                    data = (byte)fs.ReadByte();
                    isTextFile = (data != 0);
                    i++;
                }
                return isTextFile;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
        #endregion

        /// <summary>
        /// StreamWriter寫入方法
        /// </summary>
        private void StreamWriterMetod(string sPath, string sContent, string sFileName)
        {
            try
            {
                FileStream fsFile = new FileStream(sPath + "\\" + sFileName, FileMode.Create);
                StreamWriter swWriter = new StreamWriter(fsFile, Encoding.Default);
                //寫入數據
                swWriter.Write(sContent);
                swWriter.Close();
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// StreamWriter讀取方法
        /// </summary>
        private string StreamReaderMetod(string sPath)
        {
            string sLines = "";
            try
            {
                FileStream fsFile = new FileStream(sPath, FileMode.Open);
                StreamReader srReader = new StreamReader(fsFile, Encoding.Default);
                //讀取文件(讀取大文件時，最好不要用此方法)
                sLines = srReader.ReadToEnd();

                srReader.Close();
                return sLines;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void DoRename(string path, Hashtable ht, ArrayList myList)
        {
            //遞迴計數器累加1
            count++;

            //new一個目標目錄的DirectoryInfo
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            //防呆，確認目標目錄確實存在
            if (!dirInfo.Exists)
            {
                MessageBox.Show("Diretory not exist!");
                return;
            }

            //dirInfo.GetFiles()取回目錄下檔案清單(FileInfo陣列)
            FileInfo[] files = dirInfo.GetFiles();


            string keyName = "";
            foreach (FileInfo fileInfo in files)
            {
                //foreach檢查每一個FileInfo檔名，搜尋並取代檔名。
                string oldFileName = fileInfo.FullName;


                //string strExtnName=Path.GetExtension(oldFileName);//取得副檔名
                bool isTextFile = CheckIsTextFile(oldFileName);//是否為文字檔
                if (isTextFile)
                {
                    if (!fileInfo.Name.StartsWith(strReplace))
                    {
                        ht.Add(fileInfo.Name, fileInfo.FullName);//把所有檔名存進集合(鍵:檔名;值:檔路徑)
                        if (fileInfo.Name.Contains(".aspx"))
                        {

                            keyName = fileInfo.Name.Replace(".aspx", "");
                            keyName = keyName.Replace(".cs", "");
                            if (!myList.Contains(keyName))
                            {
                                myList.Add(keyName);
                            }

                        }
                    }

                }
                else
                {

                    // MessageBox.Show(fileInfo.Name + " is NOT txt file!");
                }

            }

            //dirInfo.GetDirectories()取回目錄下子目錄清單(DirectoryInfo陣列)
            DirectoryInfo[] subDirs = dirInfo.GetDirectories();
            foreach (DirectoryInfo subDirInfo in subDirs)
            {
                //遞迴呼叫DoRename()方法，傳入子目錄。
                DoRename(subDirInfo.FullName, ht, myList);
            }

            //完成本次遞迴，計數器累減1
            count--;

            if (count == 0)
            {

                //重新命名與檔案內容置換
                ReplaceAndWriteFile(ht, myList);
            }

        }

        /// <summary>
        /// 重新命名與檔案內容置換
        /// </summary>
        /// <param name="ht"></param>
        private void ReplaceAndWriteFile(Hashtable ht, ArrayList al)
        {
            btnStart.Text = "Renaming...";

            foreach (DictionaryEntry de in ht)
            {
                //計數器累加1
                count++;
                string sOrgLines = StreamReaderMetod(de.Value.ToString());//讀取檔案內容
                string tmpLines = sOrgLines;


                string tmpKeyWithExtn = "";
                tmpKeyWithExtn = de.Key.ToString();

                //////將內容特定字串取代
                for (int i = 0; i < al.Count; i++)
                {
                    //Regex.Match whole words
                    if (Regex.Match(tmpLines, @"\b" + al[i].ToString() + @"\b", RegexOptions.Singleline | RegexOptions.IgnoreCase).Success)
                    {
                        tmpLines = tmpLines.Replace(al[i].ToString(), strReplace + al[i].ToString());
                    }

                }


                //if (de.Value.ToString().IndexOf(de.Key.ToString()) != -1)
                //{
                //    //newFileName = de.Value.ToString().Replace(de.Key.ToString(), strReplace + de.Key.ToString());
                //    newFileName = strReplace + de.Key.ToString();
                //    //檔案更名
                //    //fileInfo.MoveTo(newFileName);
                //}


                // 檢查資料夾是否存在，不存在則新增 
                string strDirPath = txtPath.Text + "\\Output" + DateTime.Now.ToString("yyMMddhh") + "";


                if (!Directory.Exists(strDirPath))
                {
                    Directory.CreateDirectory(strDirPath);
                }
                //寫入新的內容到特定資料夾
                StreamWriterMetod(strDirPath, tmpLines, strReplace + tmpKeyWithExtn);

                //計數器累減1
                count--;
            }
            if (count == 0)
            {
                MessageBox.Show("Rename complete!");
                btnStart.Text = "Start Rename";
                btnStart.Enabled = true;
            }

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtPath.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //檢查txtPath，不可為空
            if (string.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("Enter or select target directory!");
                return;
            }
           
            //檢查txtSearch，不可為空
            if (string.IsNullOrEmpty(txtSearch.Text)&&cbSearch.Checked==true)
            {
                MessageBox.Show("Enter text you want to search");
                return;
            }
            if (string.IsNullOrEmpty(txtReplace.Text))
            {
                MessageBox.Show("Enter new name or replaced content  you want to do!");
                return;
            }

            //儲存搜尋、取代的字串並歸零計數器
            strSearch = txtSearch.Text;
            strReplace = txtReplace.Text;
            count = 0;

            //為避免重覆點選，重新命名處理中先停用btnStart按鈕
            btnStart.Text = "Renaming...";
            btnStart.Enabled = false;


            //呼叫重新命名方法
            Hashtable ht = new Hashtable();
            //用陣列來記錄要置換的特定字串
            ArrayList myList = new ArrayList();
            DoRename(txtPath.Text, ht, myList);
        }

        private void linkBlog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://focus1921.wordpress.com/");
        }

        private void cbSearch_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSearch.Checked == true)
            {
                txtSearch.Enabled = true;
            }
            else
            {
                txtSearch.Enabled = false;
            }

        }


    }
}
