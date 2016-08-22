using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace StbTool
{
    public class DataModel
    {
        private String name;
        private String value;
        private Object text_box;
        private int attribute; // 属性是否可读写
        public static List<DataModel> table1List = new List<DataModel>(); //面板一数据队列
        public static List<DataModel> table2List = new List<DataModel>(); //面板二数据队列
        public static List<DataModel> paramsList1 = new List<DataModel>(); //面板一数据队列
        public static List<DataModel> paramsList2 = new List<DataModel>(); //面板二数据队列
        public static List<RadioButton> rbtlist = new List<RadioButton>(); //radioButton的队列，方便数据清空
        public static List<string> timezoneList = new List<string>(); //GMT时区队列
        public static List<string> timezoneUTCList = new List<string>(); //UTC时区队列
        public static List<DataModel> playInfo1List = new List<DataModel>(); //可视化定位数据队列
        public static List<DataModel> playInfo2List = new List<DataModel>(); //可视化定位数据队列
        public static List<TextBox> info_textList = new List<TextBox>();  //一键信息收集text队列
        public static List<DataModel> network_info = new List<DataModel>();  //网络信息
        public DataModel(string name, string value, Object text, int attribute)
        {
            this.name = name;
            this.value = value;
            this.text_box = text;
            this.attribute = attribute;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public void setValue(string value)
        {
            this.value = value;
        }

        public void setObject(Object textBox)
        {
            this.text_box = textBox;
        }

        public void setAttribute(int attribute)
        {
            this.attribute = attribute;
        }

        public string getName()
        {
            return name;
        }

        public string getValue()
        {
            return value;
        }

        public object getObject()
        {
            return text_box;
        }

        public int getAttribute()
        {
            return attribute;
        }

        //初始化时区的数据
        public static void initTimeZone()
        {
            timezoneList.Add("GMT -12:00");
            timezoneList.Add("GMT -11:00");
            timezoneList.Add("GMT -10:00");
            timezoneList.Add("GMT -09:00");
            timezoneList.Add("GMT -08:00");
            timezoneList.Add("GMT -07:00");
            timezoneList.Add("GMT -06:00");
            timezoneList.Add("GMT -05:00");
            timezoneList.Add("GMT -04:00");
            timezoneList.Add("GMT -03:00");
            timezoneList.Add("GMT -02:00");
            timezoneList.Add("GMT -01:00");
            timezoneList.Add("GMT +01:00");
            timezoneList.Add("GMT +02:00");
            timezoneList.Add("GMT +03:00");
            timezoneList.Add("GMT +04:00");
            timezoneList.Add("GMT +05:00");
            timezoneList.Add("GMT +06:00");
            timezoneList.Add("GMT +07:00");
            timezoneList.Add("GMT +08:00");
            timezoneList.Add("GMT +09:00");
            timezoneList.Add("GMT +10:00");
            timezoneList.Add("GMT +11:00");
            timezoneList.Add("GMT +12:00");
            timezoneList.Add("GMT +13:00");

            timezoneUTCList.Add("UTC -12:00");
            timezoneUTCList.Add("UTC -11:00");
            timezoneUTCList.Add("UTC -10:00");
            timezoneUTCList.Add("UTC -09:00");
            timezoneUTCList.Add("UTC -08:00");
            timezoneUTCList.Add("UTC -07:00");
            timezoneUTCList.Add("UTC -06:00");
            timezoneUTCList.Add("UTC -05:00");
            timezoneUTCList.Add("UTC -04:00");
            timezoneUTCList.Add("UTC -03:00");
            timezoneUTCList.Add("UTC -02:00");
            timezoneUTCList.Add("UTC -01:00");
            timezoneUTCList.Add("UTC +01:00");
            timezoneUTCList.Add("UTC +02:00");
            timezoneUTCList.Add("UTC +03:00");
            timezoneUTCList.Add("UTC +04:00");
            timezoneUTCList.Add("UTC +05:00");
            timezoneUTCList.Add("UTC +06:00");
            timezoneUTCList.Add("UTC +07:00");
            timezoneUTCList.Add("UTC +08:00");
            timezoneUTCList.Add("UTC +09:00");
            timezoneUTCList.Add("UTC +10:00");
            timezoneUTCList.Add("UTC +11:00");
            timezoneUTCList.Add("UTC +12:00");
            timezoneUTCList.Add("UTC +13:00");
        }
    }
}
