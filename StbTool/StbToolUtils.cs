using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace StbTool
{
    class StbToolUtils
    {
        public static string GetOpenFileName()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            openFileDialog.InitialDirectory = path;//注意这里写路径时要用c:\\而不是c:\ 默认路径
            openFileDialog.Filter = "文本文件|*.txt";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            string fName = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fName = openFileDialog.FileName;
            }
            return fName;
        }

        public static string GetUpgradeZipFileName()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "文本文件|*.zip";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            string fName = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fName = openFileDialog.FileName;
            }
            return fName;
        }

        public static string GetSaveFileName()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            saveFileDialog.InitialDirectory = path;//注意这里写路径时要用c:\\而不是c:\ 默认路径
            saveFileDialog.Filter = "文本文件|*.txt";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;
            string fName = "";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fName = saveFileDialog.FileName;
            }
            return fName;
        }

        public static void WriteListToTextFile(string txtFile)
        {
            //创建一个文件流，用以写入或者创建一个StreamWriter 
            FileStream fs = new FileStream(txtFile, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            foreach (DataModel model in DataModel.table1List)
            {
                sw.WriteLine(model.getName() + ":" + model.getValue());
            }
            foreach (DataModel model in DataModel.table2List)
            {
                sw.WriteLine(model.getName() + ":" + model.getValue());
            }
            //关闭此文件t 
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        public static void WritePlayInfoToTextFile(string txtFile)
        {
            //创建一个文件流，用以写入或者创建一个StreamWriter 
            FileStream fs = new FileStream(txtFile, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            foreach (DataModel model in DataModel.playInfo1List)
            {
                sw.WriteLine(model.getName() + ":" + model.getValue());
            }
            //关闭此文件t 
            sw.Flush();
            sw.Close();
            fs.Close();
        }


        //读取文本文件转换为List 
        public static void ReadTextFileToList(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            //使用StreamReader类来读取文件 
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            // 从数据流中读取每一行，直到文件的最后一行
            string tmp = sr.ReadLine();
            if (tmp != null)
            {
                foreach (DataModel model in DataModel.table1List)
                {
                    DataModel tempModel = null; //生成新的对象，避免修改tablelist
                    if (tmp == null)
                        continue;
                    if (model.getAttribute() == 3)
                        tempModel = new DataModel(model.getName(), tmp.Substring(model.getName().Length + 1), model.getObject(), model.getAttribute());
                    else
                        tempModel = new DataModel(model.getName(), model.getValue(), model.getObject(), model.getAttribute());
                    DataModel.paramsList1.Add(tempModel);
                    tmp = sr.ReadLine();
                }
                foreach (DataModel model in DataModel.table2List)
                {
                    DataModel tempModel = null;
                    if (tmp == null)
                        continue;
                    if (model.getAttribute() == 3)
                        tempModel = new DataModel(model.getName(), tmp.Substring(model.getName().Length + 1), model.getObject(), model.getAttribute());
                    else
                        tempModel = new DataModel(model.getName(), model.getValue(), model.getObject(), model.getAttribute());
                    DataModel.paramsList2.Add(tempModel);
                    tmp = sr.ReadLine();
                }
            }
            //关闭此StreamReader对象 
            sr.Close();
            fs.Close();
        }

        //判断字符串是否为IP
        public static bool IsCorrectIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        //判断是否修改静态网络
        public static bool isModeifyStaticNet(String modifyName)
        {
            if (modifyName.Equals("connecttype") ||
                modifyName.Equals("stbIP") ||
                modifyName.Equals("gateway") ||
                modifyName.Equals("netmask") ||
                modifyName.Equals("dns"))
                return true;
            return false;
        }
    }
}
