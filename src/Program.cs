// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

using Star;

class Program {

  static void Main(string[] args) {

    Log.SetLogger(new ConsoleLogger());

    var vm = new VideoMode(800,600);
    var window = new SFWindow(vm, "Star System");

    bool shadersAvailable = SFML.Graphics.Shader.IsAvailable;
    Log.Write(shadersAvailable ? "Shaders are available" : "Shaders are NOT available.");

    StarIntRect b0 = new StarIntRect(0,0,5,5);
    StarIntRect b1 = new StarIntRect(4,4,5,5);

    Log.Write(b0.ToString());
    Log.Write(b1.ToString());

    var bint = StarIntRect.Intersection(b0,b1);

    Log.Write(bint.ToString());

    //var tokens = Star.Widget.WidgetLexer.GetTokens("breaking this\n down now.");
    /*string str = "<goal> <boat> text play </boat> </goal>";
    var tokens = Star.Widget.WidgetLexer.GetTokens(str);

    foreach (var token in tokens) {
      string output = "output";
      switch (token.tokenType) {
        case Star.Widget.TextToken.TOKENTYPE.PLAINTEXT:
          output = token.plainText + "+PLAINTEXT";
          break;
        case Star.Widget.TextToken.TOKENTYPE.NEWLINE:
          output = "+NL";
          break;
        case Star.Widget.TextToken.TOKENTYPE.SYLPHOPEN:
          output = "+OPENN " + token.sylphName;
          break;
        case Star.Widget.TextToken.TOKENTYPE.SYLPHCLOSE:
          output = "+CLOSE " + token.sylphName;
          break;
      }
      Log.Write(output);
      //Log.Write(token.plainText + "+" + token.tokenType.ToString());
    }*/

    var rectit = new IterableRect(new StarIntRect(0,0,3,3));
    foreach (var xy in rectit) {
      Log.Write($"{xy.x},{xy.y}");
    }

    



    string testString = "   this is a test of removing    spaces   ";

    string removed = Util.RemoveSpaces(testString);
    Log.Write("!" + removed + "!");

    //return;

    var gameLoop = new GameLoop(window);
    gameLoop.RunLoop();
    
    TextureLibrary.UnloadAll();

    /*Dictionary<KeyButton,string> test = new Dictionary<KeyButton, string>();

    test.Add(new KeyButton('a'), "blah");

    KeyButton kb = new KeyButton('b');
    kb.alphanumeric = 'a';

    bool contains = test.ContainsKey(kb);
    Log.Write(contains.ToString());*/


  }
}