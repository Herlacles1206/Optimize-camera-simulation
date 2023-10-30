using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace OPTCAMSim
{
    public static class GeneratePanelData
    {
        public static List<NestedPanels> LoadJson(string nestedPath)
        {
            List<NestedPanels> panelList = new List<NestedPanels>();
            try
            {

                if (!Directory.Exists(nestedPath))
                {
                    MessageBox.Show("Folder not found:" + nestedPath,"Error");
                    return null;
                }

                List<string> file_list = Directory.GetFiles(nestedPath, "*", SearchOption.TopDirectoryOnly).ToList();

                if (file_list.Count == 0)
                {
                    MessageBox.Show("Folder is empty", "Error");
                    return null;
                }
                
                //get file names Panel_1, Panel_2 etc
                
                foreach (string item in file_list)
                {
                    if (!File.Exists(item)) 
                        continue;
                    string json = File.ReadAllText(item);
                    NestedPanels panel = JsonConvert.DeserializeObject<NestedPanels>(json);
                    if (string.IsNullOrWhiteSpace(panel.Name))
                    {
                        FileInfo f = new FileInfo(item);
                        panel.Name = f.Name.Substring(0, f.Name.LastIndexOf("."));
                    }
                    panel.textTexture = new TextTexture(panel);

                    panelList.Add(panel);
                }

                return panelList;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;

            }

        }

    }

    [Serializable]
    public class NestedPartList
    {//after parts are returned from nesting in OptimaDor. This represents the json file data in Panel_1.txt....
        public NestedPartList()
        {

        }

        public int panelID { get; set; }
        public int modulID { get; set; }
        public int partID { get; set; }
        public string partName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int XA { get; set; }
        public int YA { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Rotated { get; set; }
        public bool LeftMost { get; set; }
        public bool TopMost { get; set; }
        public bool RightMost { get; set; }
        public bool BottomMost { get; set; }
        public string SideA { get; set; }//DoorModel
        public string SideB { get; set; }//DoorModelFile
    }

    public class NestedPanels
    {//do  not make Panel, confuses with windows control Panel
        public int PanelId { get; set; }
        public string Name { get; set; }
        public int PanelHeight { get; set; }
        public int PanelWidth { get; set; }
        public double PanelThickness { get; set; }

        public string Color { get; set; }
        public bool CutAllSides { get; set; }
        public List<NestedPartList> PartList { get; set; }
        public TextTexture textTexture;
        public ToolProgram toolProgram;
    }
}
