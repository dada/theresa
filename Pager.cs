
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

public class Pager : MidiControllerBase, IController, ISkinned {

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

private ArrayList _Pages;

public int LastPage {
  get { return _Pages.Count - 1; }
}

private int _CurrentPage = 0;

private BigButton _PrevPage;
private BigButton _NextPage;

public Pager(Skin skin) : base(skin) {

  _Pages = new ArrayList();

  Width = 72;
  Height = 45;

  _PrevPage = new BigButton(Skin);
  _PrevPage.Bounds = new Rectangle(4, 13, 31, 29);
  _PrevPage.Text = "<"; 
  _PrevPage.Click += new EventHandler(OnPrevPage);
  _PrevPage.Parent = this;

  _NextPage = new BigButton(Skin);
  _NextPage.Bounds = new Rectangle(39, 13, 31, 29);
  _NextPage.Text = ">"; 
  _NextPage.Click += new EventHandler(OnNextPage);
  _NextPage.Parent = this;

  this.MouseDown += new MouseEventHandler(OnMouseDown);
}

protected virtual void OnMouseDown(object sender, MouseEventArgs e) {
  if(_LcdRect.Contains(e.X, e.Y)) {
    // special configuration
    PagerPage page = (PagerPage) _Pages[_CurrentPage];
    TextInputScreen editor = Global.GetTextInputScreen(Skin);
    editor.String = page.Name;
    DialogResult r = editor.ShowDialog();
    if(r == DialogResult.OK) {
      page.Name = editor.String;
      Invalidate();
    }
  }
}

protected override string GetLcdText() {
  PagerPage page = (PagerPage) _Pages[_CurrentPage];
  return "[" + (_CurrentPage+1) + "/" + _Pages.Count + "] " + page.Name;
}

private void OnPrevPage(object sender, EventArgs e) {
  Console.WriteLine("PrevPage: now on " + _CurrentPage + "/" + (_Pages.Count-1));
  if(_CurrentPage > 0) {
    SavePage();
    LoadPage(_CurrentPage-1);
  }
  Invalidate();
}

private void OnNextPage(object sender, EventArgs e) {
  if(_CurrentPage + 1 > _Pages.Count - 1) {
    DialogResult r = MessageBox.Show(
      "Create new page?", "Theresa",
      MessageBoxButtons.YesNo,
      MessageBoxIcon.Question,
      MessageBoxDefaultButton.Button1
    );
    if(r == DialogResult.Yes) {
      SavePage();
      int i = NewPage();
      LoadPage(i);
    }
  } else {
    SavePage();
    LoadPage(_CurrentPage+1);
  }
  Invalidate();
}

private int NewPage() {  
  PagerPage p = new PagerPage();
  foreach(Control c in Parent.Controls) {
    if(c is IMidiController) {
      IMidiController controller = (IMidiController) c;
      int id = ((IController) c).ID;
      p.Add(id, controller.Config, controller.Value, controller.Value2);
    }
  }
  _Pages.Add(p);
  Console.WriteLine("Creating new page " + (_Pages.Count - 1));
  return _Pages.Count - 1;
}

public override string ToXml() {
  SavePage();
  string xml = String.Format("<Controller id=\"{0}\">\r\n", ID);  
  foreach(PagerPage p in _Pages) {
    xml += String.Format("\t<Page name=\"{0}\">\r\n", p.Name);
    foreach(Control c in Parent.Controls) {
      if(c is IMidiController) {
        IMidiController controller = (IMidiController) c;
        int id = ((IController) c).ID;
        ControllerConfig cfg = p[id].Config;
        xml += String.Format(
          "\t\t<Config id=\"{0}\" name=\"{1}\" cc=\"{2}\" value=\"{3}\" cc2=\"{4}\" value2=\"{5}\" ", 
          id, cfg.Name, cfg.CC, p[id].Value, cfg.CC2, p[id].Value2
        );
        foreach(string n in cfg.AdditionalProperties.Keys) {
          xml += String.Format("{0}=\"{1}\" ", 
            n, cfg.AdditionalProperties[n].Value
          );
        }
        xml += "/>\r\n";
      }
    }
    xml += String.Format("\t</Page>\r\n");
  }
  xml += "</Controller>";
  return xml;
}

public override void FromXml(XmlNode n) {
  _Pages.Clear();
  foreach(XmlNode p in n.ChildNodes) {
    if(p.NodeType == XmlNodeType.Element && p.Name == "Page") {
      int i = NewPage();
      PagerPage page = (PagerPage) _Pages[i];
      page.Name = p.Attributes["name"].Value;
      foreach(XmlNode c in p.ChildNodes) {
        if(c.NodeType == XmlNodeType.Element && c.Name == "Config") {
          int id = Convert.ToInt32(c.Attributes["id"].Value);
          if(c.Attributes["name"] != null) {
            page[id].Config.Name = c.Attributes["name"].Value;
          }
          if(c.Attributes["cc"] != null) {
            page[id].Config.CC = Convert.ToInt32(c.Attributes["cc"].Value);
          }
          if(c.Attributes["value"] != null) {
            page[id].Value = Convert.ToInt32(c.Attributes["value"].Value);
          }
          if(c.Attributes["cc2"] != null) {
            page[id].Config.CC2 = Convert.ToInt32(c.Attributes["cc2"].Value);
          }
          if(c.Attributes["value2"] != null) {
            page[id].Value2 = Convert.ToInt32(c.Attributes["value2"].Value);
          }
          foreach(string k in page[id].Config.AdditionalProperties.Keys) {
            if(c.Attributes[k] != null) {
              page[id].Config.AdditionalProperties[k].Value = Convert.ToInt32(c.Attributes[k].Value);
            }
          }
        }
      }
    }
  }
  LoadPage(0);
}

public override void LayoutLoaded() {
  NewPage();
  LoadPage(0);
}

private void SavePage() {
  // save current page  
  Client master = (Client) Parent;
  PagerPage page = (PagerPage) _Pages[_CurrentPage];
  foreach(Control c in master.Controls) {
    if(c is IMidiController) {
      IMidiController controller = (IMidiController) c;
      int id = ((IController) c).ID;
      page[id].Value = controller.Value;
      Console.WriteLine("saving page[" + id + "].Value=" + controller.Value);
      page[id].Value2 = controller.Value2;
      Console.WriteLine("saving page[" + id + "].Value2=" + controller.Value2);
      controller.Config.CopyTo(page[id].Config);	  
    }
  }
}

private void LoadPage() {
  LoadPage(_CurrentPage);
}

private void LoadPage(int p) {
  // load new page
  Client master = (Client) Parent;
  PagerPage page = (PagerPage) _Pages[p];
  foreach(Control c in master.Controls) {
    if(c is IMidiController) {
      IMidiController controller = (IMidiController) c;
      int id = ((IController) c).ID;
      page[id].Config.CopyTo(controller.Config);
      controller.Value = page[id].Value;
      Console.WriteLine("setting page[" + id + "].Value=" + page[id].Value);
      controller.Value2 = page[id].Value2;
      Console.WriteLine("setting page[" + id + "].Value2=" + page[id].Value2);
      master.OnControlChange(controller, new EventArgs());
    }
  }
  Console.WriteLine("CurrentPage now " + p);
  _CurrentPage = p;
}

} // class

public class ControllerState {

public ControllerConfig Config;
public int Value;
public int Value2;

public ControllerState() {
  Config = new ControllerConfig();
}

} // class

public class PagerPage {

private string _Name = String.Empty;
public string Name {
  get { return _Name; }
  set { _Name = value; }
}

private Hashtable _Config;

public PagerPage() {
  _Config = new Hashtable();
}

public void Add(int id, ControllerConfig cfg, int value, int value2) {
  ControllerState s = new ControllerState();
  cfg.CopyTo(s.Config);
  s.Value = value;
  s.Value2 = value2;
  _Config.Add(id, s);
}

public ControllerState this[int id] {
  get { return (ControllerState) _Config[id]; }
}

} // class

} // namespace
