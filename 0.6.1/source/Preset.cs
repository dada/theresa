
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Theresa {

public class Preset {

public string AppPath {
  get {
    if(System.Environment.OSVersion.Platform == PlatformID.WinCE) {
      return Path.GetDirectoryName( 
        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase
      );
    } else {
      return ".";
    }
  }
}

private Client _Master;
public Client Master {
  get { return _Master; }
  set { _Master = value; }
}

public Preset() {

}

public bool Load(string filename) {
  StreamReader file = new StreamReader(filename);
  string xml = file.ReadToEnd();
  file.Close();
  XmlDocument x = new XmlDocument();
  x.LoadXml(xml);
  foreach(XmlNode n in x.ChildNodes[0].ChildNodes) {

    if(n.NodeType == XmlNodeType.Element) {
      
      switch(n.Name) {
      case "Layout":
        Master.LoadLayout(n.Attributes["name"].Value);
        break;
      case "Skin":
        Skin skin = new Skin();
        skin.Load(n.Attributes["name"].Value);
        Master.ChangeSkin(skin);
        break;
      case "Controller":
        int id = Convert.ToInt32(n.Attributes["id"].Value);
        int cc = Convert.ToInt32(n.Attributes["cc"].Value);
        int value = Convert.ToInt32(n.Attributes["value"].Value);
        foreach(Control c in Master.Controls) {
          if(c is IMidiController) {
            IMidiController controller = (IMidiController) c;
            if(controller.ID == id) {
              controller.Config.Name = n.Attributes["name"].Value;
              controller.Config.CC = cc;
              controller.Value = value;
              Master.OnControlChange(controller, new EventArgs());
            }
          }
        }
        break;
      }
    }
  }
  return true;
}


public void Save(string filename) {
  StreamWriter file = new StreamWriter(filename);
  file.WriteLine("<Theresa file=\"preset\">");
  file.WriteLine("\t<Skin name=\"{0}\" />", Master.CurrentSkin.Name);
  file.WriteLine("\t<Layout name=\"{0}\" />", Master.CurrentLayout);
  foreach(Control c in Master.Controls) {
    if(c is IMidiController) {
      IMidiController controller = (IMidiController) c;
      file.WriteLine(
        "\t<Controller id=\"{0}\" cc=\"{1}\" name=\"{2}\" value=\"{3}\" />",
        controller.ID, controller.Config.CC, controller.Config.Name, controller.Value
      );
    }
  }
  file.WriteLine("</Theresa>");
  file.Close();
}


} // class

} // namespace