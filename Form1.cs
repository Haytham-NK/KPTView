using System.Xml.Linq;

namespace KPTView
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private XDocument xmlDoc = new XDocument();

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlDoc = XDocument.Load(ofd.FileName);
                LoadTree();
            }
        }

        private void LoadTree()
        {
            treeView1.Nodes.Clear();

            if (xmlDoc.Root is null) return;

            var roots = new Dictionary<string, TreeNode>
            {
                ["Parcels"] = new TreeNode("Parcels"),
                ["ObjectRealty"] = new TreeNode("ObjectRealty"),
                ["SpatialData"] = new TreeNode("SpatialData"),
                ["Bounds"] = new TreeNode("Bounds"),
                ["Zones"] = new TreeNode("Zones")
            };

            AddNodesToRoot("land_record", "cad_number", roots["Parcels"]);
            AddNodesToRoot("build_record", "cad_number", roots["ObjectRealty"]);
            AddNodesToRoot("construction_record", "cad_number", roots["ObjectRealty"]);
            AddNodesToRoot("entity_spatial", "sk_id", roots["SpatialData"]);
            AddNodesToRoot("municipal_boundary_record", "reg_numb_border", roots["Bounds"]);
            AddNodesToRoot("zones_and_territories_record", "reg_numb_border", roots["Zones"]);

            treeView1.Nodes.AddRange(roots.Values.ToArray());
            treeView1.ExpandAll();
        }

        private void AddNodesToRoot(string recordName, string labelElementName, TreeNode parent)
        {
            var records = xmlDoc.Descendants(recordName);
            foreach (var record in records)
            {
                var label = record.Descendants(labelElementName).FirstOrDefault()?.Value ?? "Unknown";
                parent.Nodes.Add(new TreeNode(label) { Tag = record });
            }
        }
        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Руководство по программе:\n\n" +
                "1. Нажмите 'Открыть XML' и выберите файл КПТ.\n" +
                "2. В дереве отобразятся разделы.\n" +
                "3. Нажмите на узел, чтобы увидеть подробности.\n" +
                "4. Для сохранения данных отметьте галочкой нужные узлы и нажмите кнопку 'Сохранить'.\n\n" +
                "Автор: Клиновой Никита Иванович\n" +
                "Дата выполнения: 03.09.2025 г.",
                "Помощь", MessageBoxButtons.OK, MessageBoxIcon.Information
            );
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var selectedNodes = GetSelectedNodes(treeView1.Nodes);

            if (selectedNodes.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один узел для сохранения.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "XML Files|*.xml",
                FileName = "selected_data.xml"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var root = new XElement("SelectedData");

                foreach (var node in selectedNodes)
                {
                    if (node.Tag is XElement el)
                        root.Add(new XElement(el));
                }

                new XDocument(root).Save(sfd.FileName);
                MessageBox.Show("Файл успешно сохранен.");
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is XElement element)
            {
                rtbElements.Text = FormatElement(element, 0);
            }
        }

        private string FormatElement(XElement element, int indent)
        {
            string indentStr = new string(' ', indent * 2);
            string result = "";

            foreach (var child in element.Elements())
            {
                if (!child.HasElements)
                {
                    result += $"{indentStr}{child.Name.LocalName}: {child.Value}\n";
                }
                else
                {
                    result += $"{indentStr}{child.Name.LocalName}:\n";
                    result += FormatElement(child, indent + 1);
                }
            }

            return result;
        }

        private List<TreeNode> GetSelectedNodes(TreeNodeCollection nodes)
        {
            var result = new List<TreeNode>();

            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is XElement)
                    result.Add(node);

                if (node.Nodes.Count > 0)
                    result.AddRange(GetSelectedNodes(node.Nodes));
            }

            return result;
        }
    }
}
