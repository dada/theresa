using System;

namespace Theresa {

public class Global {

private static TextInputScreen _TextInputScreen;
private static NumberInputScreen _NumberInputScreen;
private static SelectionScreen _SelectionScreen;

public static TextInputScreen GetTextInputScreen(Skin skin) {
  if(_TextInputScreen == null) {
    _TextInputScreen = new TextInputScreen(skin);
  } else {
    if(_TextInputScreen.Skin != skin) {
      _TextInputScreen.Skin = skin;
    }
  }
  return _TextInputScreen;
}

public static NumberInputScreen GetNumberInputScreen(Skin skin) {
  if(_NumberInputScreen == null) {
    _NumberInputScreen = new NumberInputScreen(skin);
  } else {
    if(_NumberInputScreen.Skin != skin) {
      _NumberInputScreen.Skin = skin;
    }
  }
  return _NumberInputScreen;
}

public static SelectionScreen GetSelectionScreen(Skin skin) {
  if(_SelectionScreen == null) {
    _SelectionScreen = new SelectionScreen(skin);
  } else {
    if(_SelectionScreen.Skin != skin) {
      _SelectionScreen.Skin = skin;
    }
  }
  return _SelectionScreen;
}

} // class

} // namespace
