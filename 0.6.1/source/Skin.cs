
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Theresa {

public class Skin {

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

private string _Name;
public string Name {
  get { return _Name; }
}

public Color BackgroundColor;
public Bitmap Background;
public Bitmap FaderForeground;
public Bitmap FaderBackground;
public Bitmap ButtonOn;
public Bitmap ButtonOff;
public Brush LcdForeground;
public Brush LcdBackground;
public Pen LcdBorder;

public Skin() {

}

private void Init() {
  BackgroundColor = Color.White;
  Background = null;
  FaderForeground = null;
  FaderBackground = null;
  ButtonOn = null;
  ButtonOff = null;
  LcdForeground = new SolidBrush(Color.Lime);
  LcdBackground = new SolidBrush(Color.Black);
  LcdBorder = new Pen(Color.Green);
}

public bool Load(string name) {
  Init();
  string filename = AppPath + "\\Skins\\" + name + ".xml";
  StreamReader file = new StreamReader(filename);
  string xml = file.ReadToEnd();
  file.Close();
  XmlDocument x = new XmlDocument();
  x.LoadXml(xml);
  foreach(XmlNode n in x.ChildNodes[0].ChildNodes) {

    if(n.NodeType == XmlNodeType.Element) {
      
      switch(n.Name) {
      case "Background":
        if(n.Attributes["bitmap"] != null) {
          Background = new Bitmap(AppPath + "\\Skins\\" + n.Attributes["bitmap"].Value);
        }
        if(n.Attributes["color"] != null) {
          BackgroundColor = StringToColor(n.Attributes["color"].Value);
        }
        break;
      case "Fader":
        if(n.Attributes["foreground"] != null) {
          FaderForeground = new Bitmap(AppPath + "\\Skins\\" + n.Attributes["foreground"].Value);
        }
        if(n.Attributes["background"] != null) {
          FaderBackground = new Bitmap(AppPath + "\\Skins\\" + n.Attributes["background"].Value);
        }
        break;
      case "Button":
        if(n.Attributes["on"] != null) {
          ButtonOn = new Bitmap(AppPath + "\\Skins\\" + n.Attributes["on"].Value);
        }
        if(n.Attributes["off"] != null) {
          ButtonOff = new Bitmap(AppPath + "\\Skins\\" + n.Attributes["off"].Value);
        }
        break;
      case "LCD":
        if(n.Attributes["foreground"] != null) {
          LcdForeground = new SolidBrush(StringToColor(n.Attributes["foreground"].Value));
        }
        if(n.Attributes["background"] != null) {
          LcdBackground = new SolidBrush(StringToColor(n.Attributes["background"].Value));
        }
        if(n.Attributes["border"] != null) {
          LcdBorder = new Pen(StringToColor(n.Attributes["border"].Value));
        }
        break;
      }
    }
  }
  _Name = name;
  return true;
}

private Color StringToColor(string name) {
  if(name.IndexOf(",") != -1) {
    string[] rgb = name.Split(',');
    return Color.FromArgb(
      Convert.ToInt32(rgb[0]),
      Convert.ToInt32(rgb[1]),
      Convert.ToInt32(rgb[2])
    );
  } else {
    PropertyInfo pi = typeof(Color).GetProperty(name);
    if(pi != null) {
      return (Color) pi.GetValue(null, null);
    } else {
      return Color.Empty;
    }
  }
}

} // class

interface ISkinned {

Skin Skin { get; set; }

} // interface

} // namespace