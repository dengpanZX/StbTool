using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace StbTool
{
    class DataModel
    {
        private String name;
        private String value;
        private Object text_box;
        public static List<DataModel> table1List = new List<DataModel>();
        public static List<DataModel> table2List = new List<DataModel>();

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
    }
}
