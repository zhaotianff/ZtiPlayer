using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

namespace ZtiPlayer.Utils
{
    public class XmlHelper
    {
        XDocument doc;
        string filePath;

        public XmlHelper(string filePath)
        {
            this.filePath = filePath;
            OpenFile(filePath);
        }

        public XmlHelper()
        {

        }

        public void OpenFile(string filePath)
        {            
            this.doc = XDocument.Load(filePath);                         
        }

        public IEnumerable<XElement> GetValueByXPath(string xpath)
        {
            IEnumerable<XElement> list = new List<XElement>();
            list = doc.XPathSelectElements(xpath);
            return list;
        }

        public void GetValueByXPath(string xpath,string attributeName,out string nodeValue,out string nodeAttribute)
        {
            var result = doc.XPathSelectElement(xpath);
            if(result != null)
            {
                nodeValue = result.Value;
                if(!string.IsNullOrEmpty(attributeName))
                {
                    nodeAttribute = result.Attribute(attributeName).Value;
                }
                else
                {
                    nodeAttribute = "";
                }
                
            }
            else
            {
                nodeValue = "";
                nodeAttribute = "";
            }
        }
    }
}
