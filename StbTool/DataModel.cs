﻿using System;
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
        public static List<DataModel> table1List = new List<DataModel>();
        public static List<DataModel> table2List = new List<DataModel>();
        public static List<RadioButton> rbtlist = new List<RadioButton>();
        public static List<string> timezoneList = new List<string>();
        public static List<string> timezoneUTCList = new List<string>();

        public DataModel(string name, string value, Object text)
        {
            this.name = name;
            this.value = value;
            this.text_box = text;
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
