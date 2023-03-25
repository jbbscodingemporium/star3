namespace Star.Widget {

  public class LeetSylph : Sylph {

    List<bool> flipCell = new List<bool>();
    List<int> updatesUntilChange = new List<int>();

    List<XY> cellsToUpdateThisFrame = new List<XY>();

    public LeetSylph(string name, int depth) : base(name, depth) {}

    private void ResizeUnitsTracked() {
      int newSize = xys.Count;
      while (updatesUntilChange.Count > newSize) {
        updatesUntilChange.RemoveAt(updatesUntilChange.Count-1);
      }
      while (updatesUntilChange.Count < newSize) {
        updatesUntilChange.Add(0);
      }
      while (flipCell.Count > newSize) {
        flipCell.RemoveAt(flipCell.Count-1);
      }
      while (flipCell.Count < newSize) {
        flipCell.Add(true);
      }
    }

    public override void Update(TextWidget.TextCellData data) {
      ResizeUnitsTracked();

      for (int i = 0; i < updatesUntilChange.Count; ++i) {
        updatesUntilChange[i]--;
        if (updatesUntilChange[i] <= 0) {
          updatesUntilChange[i] = 30;
          flipCell[i] = !flipCell[i];
        }
      }

      for (int i = 0; i < xys.Count; ++i) {
        if (flipCell[i] == false) continue;
        int x = xys[i].x;
        int y = xys[i].y;
        char ch = data.GetCharacter(x,y);   //Will grab the override if there is one.
        ch = Char.ToLower(ch);

        if (LeetConversions.ContainsKey(ch)) {
          char after = LeetConversions[ch];
          data.SetOverrideCharacter(x,y,after);
        }
        
      }

    }

    /*public void UpdateOld(TextWidget.TextCellData data) {
      ResizeUnitsTracked();

      for (int i = 0; i < updatesUntilChange.Count; ++i) {
        updatesUntilChange[i]--;
        if (updatesUntilChange[i] <= 0) {
          updatesUntilChange[i] = 30;
          cellsToUpdateThisFrame.Add(xys[i]);
        }
      }

      foreach (var xy in cellsToUpdateThisFrame) {
        int x = xy.x;
        int y = xy.y;
        char ch = data.GetCharacter(x,y);     //Will grab the override character if there is one.
        ch = Char.ToLower(ch);

        char after = ch;
        if (LeetConversions.ContainsKey(ch)) { after = LeetConversions[ch];}

        if (after != ch) {
          data.SetOverrideCharacter(x,y,after);
        }
      }

      cellsToUpdateThisFrame.Clear();
    }*/

    //Returns null if you can't generate anything.
    public static LeetSylph? GenerateFromString(string name, int depth, Dictionary<string,string> tags) {

      if (name == "leet" || name == "l33t") {
        return new LeetSylph(name,depth);
      }

      return null;
    }

    static Dictionary<char,char> LeetConversions = new Dictionary<char, char>() {
      {'a','4'},
      {'4','a'},
      {'i','1'},
      {'1','i'},
      {'3','e'},
      {'e','3'},
      {'0','o'},
      {'o','0'}
    };

  }
}