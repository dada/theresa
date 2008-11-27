
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

public class SelectionScreen : ScreenBase {

private ArrayList _Items;
public ArrayList Items {
  get { return _Items; }
}

private BigButton[] _Item;
private BigButton _PrevPage;
private BigButton _NextPage;
private BigButton _Cancel;

private int _Page;

private int _SelectedIndex = -1;
public int SelectedIndex {
  get { return _SelectedIndex; }
}

public SelectionScreen(Skin skin) : base(skin) {

  _Items = new ArrayList();
  _Page = 0;
  
  _Item = new BigButton[7];
  for(int i = 0; i < 7; i++) {
    _Item[i] = new BigButton(Skin);
    _Item[i].Bounds = new Rectangle(10, 10+i*40, ClientSize.Width-20, 32);
    _Item[i].Click += new EventHandler(OnChoose);
    _Item[i].Parent = this;
  }
  
  _PrevPage = new BigButton(Skin);
  _PrevPage.Bounds = new Rectangle(10, 290, ClientSize.Width/4-20, 32);
  _PrevPage.Text = "<";
  _PrevPage.Click += new EventHandler(OnPrevPage);
  _PrevPage.Parent = this;
  
  _NextPage = new BigButton(Skin);
  _NextPage.Bounds = new Rectangle(_PrevPage.Bounds.Right+10, 290, ClientSize.Width/4-20, 32);
  _NextPage.Text = ">";
  _NextPage.Click += new EventHandler(OnNextPage);
  _NextPage.Parent = this;

  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(ClientSize.Width/2+5, 290, ClientSize.Width/2-15, 32);
  _Cancel.Text = "Cancel";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;

}

public void ShowPage() {
  for(int i = 0; i < 7; i++) {
    int n = 7*_Page+i;
    if(n < _Items.Count) {
      _Item[i].Text = _Items[n].ToString();
      _Item[i].Visible = true;
    } else {
      _Item[i].Visible = false;
    }
  }  
}

private void OnChoose(object sender, EventArgs e) {
  int n = -1;
  for(int i = 0; i < 7; i++) {
    if(sender == _Item[i]) {
      n = i;
    }
  }
  if(n == -1) {
    DialogResult = DialogResult.Cancel;
  } else {
    _SelectedIndex = 7*_Page+n;
    DialogResult = DialogResult.OK;
  }
  Close();
}

private void OnPrevPage(object sender, EventArgs e) {
  if(_Page > 0) {
    _Page--;
    ShowPage();
  }
}

private void OnNextPage(object sender, EventArgs e) {
  if(7*(_Page+1) < _Items.Count) {
    _Page++;
    ShowPage();
  }
}

private void OnCancel(object sender, EventArgs e) {
  DialogResult = DialogResult.Cancel;
  Close();
}

} // class

} // namespace