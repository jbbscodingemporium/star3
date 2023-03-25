namespace Star.Widget {

  public class RandomCapsSylph : Sylph {

    List<bool> flipCaps = new List<bool>();
    List<int> updatesUntilChange = new List<int>();

    List<XY> cellsToUpdateThisFrame = new List<XY>();

    public RandomCapsSylph(string name, int depth) : base(name, depth) {}

    private void ResizeUnitsTracked() {
      int newSize = xys.Count;
      while (flipCaps.Count > newSize) {
        flipCaps.RemoveAt(flipCaps.Count-1);
      }
      while (updatesUntilChange.Count > newSize) {
        updatesUntilChange.RemoveAt(updatesUntilChange.Count-1);
      }
      while (flipCaps.Count < newSize) {
        flipCaps.Add(false);
      }
      while (updatesUntilChange.Count < newSize) {
        updatesUntilChange.Add(0);
      }
    }

    public override void Update(TextWidget.TextCellData data) {
      ResizeUnitsTracked();

      for (int i = 0; i < updatesUntilChange.Count; ++i) {
        updatesUntilChange[i]--;
        if (updatesUntilChange[i] <= 0) {
          updatesUntilChange[i] = 30;
          flipCaps[i] = !flipCaps[i];
        }
      }

      for (int i = 0; i < xys.Count; ++i) {
        if (!flipCaps[i]) continue;
        int x = xys[i].x;
        int y = xys[i].y;
        char before = data.GetCharacter(x,y);
        bool isUppercase = Char.IsUpper(before);
        char after = isUppercase ? Char.ToLower(before) : Char.ToUpper(before);
        if (after != before) {
          data.SetOverrideCharacter(x,y,after);
        }
      }
    }

    /*public void UpdateOldThing(TextWidget.TextCellData data) {
      ResizeUnitsTracked();

      for (int i = 0; i < updatesUntilChange.Count; ++i) {
        if (updatesUntilChange[i] <= 0) {
          //flipCaps[i] = !flipCaps[i];
          updatesUntilChange[i] = 33000;
          cellsToUpdateThisFrame.Add(xys[i]);
        }
        updatesUntilChange[i]--;
      }

      foreach (var xy in cellsToUpdateThisFrame) {
        int x = xy.x;
        int y = xy.y;
        char ch = data.GetCharacter(x,y);     //Will grab the override character if there is one.
        bool uppercase = Char.IsUpper(ch);
        char after = uppercase ? Char.ToLower(ch) : Char.ToUpper(ch);
        if (after != ch) {
          data.SetOverrideCharacter(x,y,after);
        }
      }

      cellsToUpdateThisFrame.Clear();
    }*/

    //Returns null if you can't generate anything.
    public static RandomCapsSylph? GenerateFromString(string name, int depth, Dictionary<string,string> tags) {

      if (name == "randomcaps" || name == "randcaps") {
        return new RandomCapsSylph(name,depth);
      }

      return null;
    }

  }
  
}