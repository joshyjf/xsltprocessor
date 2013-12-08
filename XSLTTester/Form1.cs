using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.Drawing;

namespace XSLTTester
{
    public partial class Form1 : Form
    {
        public static string SearchString = string.Empty;
        int start = 0;
        int indexOfSearchText = 0;

        public Form1()
        {
            InitializeComponent();
        }

        public static string xslSchemaPath { get; set; }
        public static string XMLPath { get; set; }
        public string SchemaNamespace = ConfigurationManager.AppSettings["SchemaNamespace"];
        private string schemaPath = ConfigurationManager.AppSettings["Schema"];


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            textBox1.Text = XMLPath = ofd.FileName;
            richTextBox1.Text = File.ReadAllText(ofd.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd2 = new OpenFileDialog();
            ofd2.ShowDialog();
            textBox2.Text = xslSchemaPath = ofd2.FileName;
            richTextBox2.Text = File.ReadAllText(ofd2.FileName);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// TRANSFORM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<HTML><BODY>");
            if (ValidateXml())
            {
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(richTextBox1.Text)))
                using (XmlReader xslReader = XmlReader.Create(new StringReader(richTextBox2.Text)))
                {
                    XPathNavigator xmlNavigator = new XPathDocument(xmlReader).CreateNavigator();
                    XPathNavigator xslNavigator = new XPathDocument(xslReader).CreateNavigator();
                    XslCompiledTransform transform = new XslCompiledTransform();
                    transform.Load(xslNavigator, XsltSettings.TrustedXslt, new XmlUrlResolver());
                    MemoryStream ms = new MemoryStream();
                    transform.Transform(xmlNavigator, null, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    treeXml.Nodes.Clear();
                    // Load the XML Document
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(new StreamReader(ms).ReadToEnd());

                    ConvertXmlNodeToTreeNode(doc, treeXml.Nodes);
                    treeXml.Nodes[0].ExpandAll();
                    treeXml.LabelEdit = true;
                    treeXml.Nodes[0].BeginEdit(); 
                    tabControl1.SelectedTab = tabPage3;
                }
            }
        }

        /// <summary>
        /// Validate the input xml blob against the given POSLOG schema
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>true or false as per the result of xml</returns>
        public bool ValidateXml()
        {
            bool isValid = false;
            SchemaPath = schemaPath;
            if (string.Compare(ConfigurationManager.AppSettings["isValidateXml"], "true",
                               StringComparison.OrdinalIgnoreCase) == 0)
            {
                string xml = File.ReadAllText(XMLPath);

                isValid = false;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                XmlSchemaSet xss = new XmlSchemaSet();
                xss.Add(SchemaNamespace, SchemaPath);
                settings.Schemas = xss;

                using (XmlReader reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    while (reader.Read()) ;
                    isValid = true;
                }
            }
            return isValid;
        }

        protected string SchemaPath
        {
            get;
            set;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Checked)
            {
                string[] xmlfiles = Directory.GetFiles(Environment.CurrentDirectory, "*.xml");
                string[] schemafiles = Directory.GetFiles(Environment.CurrentDirectory, "*.xsl");

                if (xmlfiles.Length > 0 & schemafiles.Length > 0)
                {
                    textBox1.Text = XMLPath = xmlfiles[0];
                    textBox2.Text = xslSchemaPath = schemafiles[0];
                    richTextBox1.Text = File.ReadAllText(XMLPath);
                    richTextBox2.Text = File.ReadAllText(xslSchemaPath);
                }
                else
                {
                    MessageBox.Show("Files missing!");
                }
            }
        }



        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //show user control
                Form2 form2 = new Form2();
                form2.ShowDialog();
                int startindex = 0;
                
                if (SearchString.Length > 0)
                    startindex = FindMyText(SearchString.Trim(), start, richTextBox1.Text.Length,sender as RichTextBox);

                // If string was found in the RichTextBox, highlight it
                if (startindex >= 0)
                {
                    // Set the highlight color as red
                    richTextBox1.SelectionColor = Color.Red;
                    // Find the end index. End Index = number of characters in textbox
                    int endindex = SearchString.Length;
                    // Highlight the search string
                    richTextBox1.Select(startindex, endindex);
                    richTextBox1.ScrollToCaret();

                    // mark the start position after the position of
                    // last search string
                    start = startindex + endindex;
                }
            }
        }


        public int FindMyText(string txtToSearch, int searchStart, int searchEnd,RichTextBox rtb)
        {
            // Unselect the previously searched string
            if (searchStart > 0 && searchEnd > 0 && indexOfSearchText >= 0)
            {
                rtb.Undo();               
            }

            // Set the return value to -1 by default.
            int retVal = -1;

            // A valid starting index should be specified.
            // if indexOfSearchText = -1, the end of search
            if (searchStart >= 0 && indexOfSearchText >= 0)
            {
                // A valid ending index
                if (searchEnd > searchStart || searchEnd == -1)
                {
                    // Find the position of search string in RichTextBox
                    indexOfSearchText = rtb.Find(txtToSearch, searchStart, searchEnd, RichTextBoxFinds.None);
                    // Determine whether the text was found in richTextBox1.
                    if (indexOfSearchText != -1)
                    {
                        // Return the index to the specified search text.
                        retVal = indexOfSearchText;
                    }
                }
            }
            return retVal;
        }



        private void richTextBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //show user control
                Form2 form2 = new Form2();
                form2.ShowDialog();
                int startindex = 0;

                if (SearchString.Length > 0)
                    startindex = FindMyText(SearchString.Trim(), start, richTextBox2.Text.Length, sender as RichTextBox);

                // If string was found in the RichTextBox, highlight it
                if (startindex >= 0)
                {
                    // Set the highlight color as red
                    richTextBox2.SelectionColor = Color.Red;
                    // Find the end index. End Index = number of characters in textbox
                    int endindex = SearchString.Length;
                    // Highlight the search string
                    richTextBox2.Select(startindex, endindex);
                    richTextBox2.ScrollToCaret();

                    // mark the start position after the position of
                    // last search string
                    start = startindex + endindex;
                }
               
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
            richTextBox2.SelectionColor = Color.Black;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = string.Empty;
            richTextBox2.Text = string.Empty;
            checkBox1.Checked = false;
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;

        }

        

        private void ConvertXmlNodeToTreeNode(XmlNode xmlNode,TreeNodeCollection treeNodes)
        {

            TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);

            switch (xmlNode.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    newTreeNode.Text = "<?" + xmlNode.Name + " " +
                      xmlNode.Value + "?>";
                    break;
                case XmlNodeType.Element:
                    newTreeNode.Text = "<" + xmlNode.Name + ">";
                    break;
                case XmlNodeType.Attribute:
                    newTreeNode.Text = "ATTRIBUTE: " + xmlNode.Name;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    newTreeNode.Text = xmlNode.Value;
                    break;
                case XmlNodeType.Comment:
                    newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
                    break;
            }

            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
                }
            }
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
            }
        }
    }

    
}
