namespace Star.Widget {

  public class TextToken {

    public enum TOKENTYPE {
      PLAINTEXT,
      WHITESPACE,
      NEWLINE,
      SYLPHOPEN,
      SYLPHCLOSE
    }

    public TOKENTYPE tokenType = TOKENTYPE.PLAINTEXT;

    //Used PLAINTEXT and WHITESPACE (for whitespace it's " ")
    public string plainText = "";

    //Used in SYLPHS only    
    public string sylphName = "";

    //Used in SYLPHOPEN only
    public Dictionary<string,string>? tags = null;

    //Used in all tokens
    public bool doneWithThisToken = false;

    public bool IsAWhiteSpaceToken() => tokenType == TOKENTYPE.WHITESPACE || tokenType == TOKENTYPE.NEWLINE;

    //Only plaintext occupies non-empty space.
    public bool OccupiesNonEmptySpace() => tokenType == TOKENTYPE.PLAINTEXT;

    public int PlainTextLength() {
      if (tokenType == TOKENTYPE.WHITESPACE) return 1;
      if (tokenType != TOKENTYPE.PLAINTEXT) return 0;
      return plainText.Length;
    }

    public TextToken(string plainText) {
      this.plainText = plainText;
      this.tokenType = TOKENTYPE.PLAINTEXT;
    }

    public TextToken(TOKENTYPE tokenType) {
      this.plainText = "";
      this.tokenType = tokenType;
    }

    static public TextToken GenerateWhitespaceToken() {
      TextToken whitespaceToken = new TextToken(TOKENTYPE.WHITESPACE);
      whitespaceToken.plainText = " ";
      return whitespaceToken;
    }

    static public TextToken GenerateSylphToken(string str) {

      //Remove any whitespace.
      str = Util.RemoveSpaces(str);

      if (str == null || str.Length == 0) {
        throw new StarExcept("Error: Tried to GenerateSylphToken with an empty string!");
      }

      bool closeToken = str[0] == '/';
      TextToken token = closeToken ? new TextToken(TOKENTYPE.SYLPHCLOSE) : new TextToken(TOKENTYPE.SYLPHOPEN);
      token.tags = new Dictionary<string, string>();

      var splitString = str.Split(',');

      if (splitString.Length == 0) {
        throw new StarExcept("Error: Tried to GenerateSylphToken with a string with only commas!");
      }

      string sylphName = splitString[0].ToLower();
      sylphName = sylphName.Replace("/", "");
      token.sylphName = sylphName;

      //Now construct the dictionary.
      for (int i = 1; i < splitString.Length; ++i) {
        string tagString = splitString[i];
        if (tagString.Length == 0) continue;
        string key = "";
        string value = "";
        if (tagString.Contains(':')) {
          var keyValuePair = tagString.Split(':');
          if (keyValuePair.Length < 2) {
            throw new StarExcept($"Error! Cannot extract key-value pair from tag: '{tagString}'");
          }
          key = keyValuePair[0];
          value = keyValuePair[1];
        } else {
          key = tagString;
        }
        token.tags.Add(key, value);
        Log.Write($"%%%%%%%%%%%%ADDED KEY VALUE: {key}, {value}");
      }

      return token;
    }


  }
  
  public static class WidgetLexer {
    static public List<TextToken> GetTokens(string str) {
      List<TextToken> tokens = new List<TextToken>();
      if (str == null) return tokens;

      string currentPlainText = "";
      for (int i = 0; i < str.Length; ++i) {
        char character = str[i];
        switch (character) {
          case ' ':
            //Convert the existing (non-whitespace) word to a single token.
            ConvertStringToTokenIfNotEmpty(ref currentPlainText, tokens);
            /*if (tokens.Count > 0) {
              tokens[tokens.Count-1].whiteSpaceAfterToken = 1;
            }*/
            //And then add the whitespace token.
            tokens.Add(TextToken.GenerateWhitespaceToken());
            break;
          case '\n':
            ConvertStringToTokenIfNotEmpty(ref currentPlainText, tokens);
            tokens.Add(new TextToken(TextToken.TOKENTYPE.NEWLINE));
            break;
          case '<':
            //First, take any current string we have and turn it into a token.
            ConvertStringToTokenIfNotEmpty(ref currentPlainText, tokens);
            //Now get the sylphtoken and put into your set of tokens.
            TextToken sylphToken;
            i = GetSylphTokenDefinition(str, i, out sylphToken);
            tokens.Add(sylphToken);
            break;
          default:
            currentPlainText += character;
          break;
        }
      }
      //Turn any remaining text at the end of the string into a token.
      ConvertStringToTokenIfNotEmpty(ref currentPlainText, tokens);

      return tokens;
    }

    //Returns the new value of the index i
    static private int GetSylphTokenDefinition(string str, int i_start, out TextToken token) {
      int closeLocation = str.IndexOf('>', i_start);
      if (closeLocation < 0) {
        throw new StarExcept("Error: cannot find close brackets in string" + str);
      }

      int contentsLength = closeLocation - i_start - 1; //It's -1 instead of +1 because you lose 2 from the < > brackets.
      string contents = str.Substring(i_start + 1, contentsLength);

      token = TextToken.GenerateSylphToken(contents);

      return closeLocation;
    }
  
    //Sets tokenstring to "".
    //If the tokenstring isn't empty then it creates a token for it.
    //Returns the token you just created or null.
    static private void ConvertStringToTokenIfNotEmpty(ref string tokenstring, List<TextToken> tokens) {
      if (tokenstring == "") return;
      TextToken token = new TextToken(tokenstring);
      //token.plainTextWhiteSpaceAtEnd = whiteSpaceAtEnd ? 1 : 0;
      tokens.Add(token);
      tokenstring = "";
    }

    static public int GetLengthToNextWhiteSpace(List<TextToken> tokens) {
      int length = 0;
      for (int i = 0; i < tokens.Count; ++i) {
        if (tokens[i].IsAWhiteSpaceToken()) break;
        length += tokens[i].PlainTextLength();
      }
      return length;
    }

  }

}