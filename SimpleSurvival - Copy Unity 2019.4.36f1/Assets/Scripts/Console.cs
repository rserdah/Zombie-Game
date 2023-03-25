using System;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    private static char quote = '"';
    /// <summary>
    /// The maximum number of arguments in a command including the base command
    /// </summary>
    private static int m_arglimit = 10;
    private static int arglimit { get => m_arglimit + 1; }

    private static int commandhistorylimit = 50;

    private static Color cyan = Color.cyan;
    private static Color grey = Color.grey;
    private static Color gray = Color.gray;
    private static Color magenta = Color.magenta;
    private static Color red = Color.red;
    private static Color yellow = Color.yellow;
    private static Color black = Color.black;
    private static Color white = Color.white;
    private static Color blue = Color.blue;
    private static Color tocolor(string color) { switch(color) { case "cyan": return cyan; case "grey": return grey; case "gray": return gray; case "magenta": return magenta; case "red": return red; case "yellow": return yellow; case "black": return black; case "white": return white; case "blue": return blue; } return white; }




    private string[] ch;
    private /*static*/ int chp; //command history pointer
    private /*static*/ string chcpy; //command history copy - when they select a command in the history, it copies to here so when they edit it and execute it, the original is preserved
    private /*static*/ int chsp; //command history size pointer

    //currently typed line, used so that if user types something, changes their mind and scrolls up to history, but then changes their mind again to go back to what they typed, they don't lose what they
    //typed
    private static string currline;

    public static Console instance;
    private InputField commandline;
    private Transform prompt;
    private ScrollRect consolelog;
    private Transform logstack;


    private void Start()
    {
        if(!instance) instance = this;
        instance.ch = new string[commandhistorylimit];

        commandline = TransformHelper.FindRecursive(transform, "commandline").GetComponent<InputField>();
        prompt = TransformHelper.FindRecursive(transform, "prompt");
        consolelog = TransformHelper.FindRecursive(transform, "consolelog").GetComponent<ScrollRect>();
        logstack = TransformHelper.FindRecursive(transform, "logstack");

        exec("print \"Hello, World!\" white");
        exec("print \"colors of the console rainbow\" white");


        //exec("\"print\" \" hi hello \" \" ' i '\\ / \" red red");

        //Debug.LogError("print \" i \" red".IndexOf("\""));

        string[] colors = { nameof(cyan), nameof(grey), nameof(gray), nameof(magenta), nameof(red), nameof(yellow), nameof(black), nameof(white), nameof(blue) };

        foreach(string s in colors)
            exec($"print {s} {s}");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
            shiftarr(ref instance.ch);

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(chp > 0)
            {
                ////If user presses up and they were typing a new command, save the command in the next command history slot
                //if(chp == chsp)
                //    ch[chsp] = instance.commandline.text;

                ////If the command history copy is not empty, replace the possibly edited command history line with the original copy
                //if(chcpy.Length > 0)
                //    ch[chp] = chcpy;

                //chp--;
                //chcpy = ch[chp];

                instance.commandline.text = ch[--chp];
            }
        }

        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(chp < chsp)
            {
                ////If the command history copy is not empty, replace the possibly edited command history line with the original copy
                //if(chcpy.Length > 0)
                //    ch[chp] = chcpy;

                //chp++;
                //chcpy = ch[chp];

                if(chp < commandhistorylimit - 1) instance.commandline.text = ch[++chp];
            }
        }
    }

    public static int exec(string command)
    {
        //This can allow for the commands FROM THE COMMAND LINE to have escaped "s just by replacing them with escaped 's
        //But it would not work for commands FROM THE CODE via exec() because it will 'consume' the escape characters rather than adding them as if they were received FROM THE COMMAND LINE from the user
        //One possible fix is to have an option in exec to tell it if it is directly from the code or from the commandline in order to know exactly how the escapes should behave
        //But for now just going to disallow user escapes completely (still have to work with the implicit/auto escapes however)
        //////////////////////////////command = command.Replace("\\\"", "\\\'");

        string[] args = getargs(command);

        try { execcommand(args); }
        catch
        {
            ///////exec($"print 'exit code 1 :('");
            exec($"print \"exitcode 1 :(\" red");
            return 1; //exit code 1: failure
        }

        //////////if(!args[0].Equals("print")) exec($"print '{command}'");
        return 0; //exit code 0: success
    }

    public void execline()
    {
        if(instance.commandline.text.Length > 0)
        {
            exec(instance.commandline.text);

            if(chsp < commandhistorylimit)
            {
                instance.ch[chp = chsp++] = instance.commandline.text;
                chp = chsp;
            }

            instance.commandline.text = "";
        }

        //Do this outside the if statement in case they enter an empty command so command line will still stay selected even if they enter an empty command
        //Make sure the cursor stays in the input field after they press enter
        instance.commandline.Select();
        instance.commandline.ActivateInputField();
    }

    private static void execcommand(params string[] args)
    {
        if(args.Length >= 1)
        {
            string command = args[0];

            switch(command)
            {
                case "give":
                    break;

                case "tel":
                    break;

                case "kill":
                    break;

                case "console":
                    break;

                case "log":
                    break;

                case "err":
                    break;

                case "print":
                    print(args[1], tocolor(args[2]));
                    break;

                default:
                    exec($"print \"command '{command}' not recognized\" red");
                    Debug.LogError($"print '{command} command not recognized'");
                    throw new System.Exception();
            }
        }
    }

    /// <summary>
    /// Returns a string array of passed arguments with the string at index 0 being the command name
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private static string[] getargs(string command)
    {
        string[] args = new string[arglimit];
        string arg;

        //Old
        //string str = normalizestringlitsymbols(command);

        string str = command;

        int i = 0, argc = 0; //argc = argument count
        while(i >= 0 && i < str.Length)
        {
            arg = getnextarg(str, ref i);

            if(arg.Length > 0 && argc < arglimit)
            {
                args[argc++] = arg;
            }

            i++;
        }

        //If there were extra allotted strings in the args array, simply copy the array only up to how many args there actually were
        string[] argstmp = null;
        if(argc > 0)
        {
            argstmp = new string[argc];
            Array.Copy(args, argstmp, argc);
        }

        return argstmp;
    }

    private static string getnextarg(string command, ref int start)
    {
        int i;
        string arg;
        char sep = getnextseparator(command, start, out i);

        if(i >= 0)
        {
            //If a space is found before a quote, then just return the substring from start to the space
            if(sep == ' ')
            {
                arg = substr(command, start, i);
            }
            //Else a quote is found before a space, so return the string literal that it contains
            else
            {
                arg = extractstringlit(command, ref start);
                i += arg.Length + 1; //When extracting a string literal, must increment i by the length of the string (plus one for the quote) in order to be able to get to the next arg
            }

            start = i;
            return arg;
        }

        //If next space was not found, then reached the last arg so return the substring from start to the end of string
        if(i < 0)
        {
            arg = substr(command, start, command.Length);
            start = command.Length - 1;

            return arg;
        }

        return "";
    }

    private static char getnextseparator(string str, int start, out int idx)
    {
        int spaceidx = str.IndexOf(' ', start);
        int quoteidx = indexofquote(str, start);

        //If no space and no quote found, then there is no separator so return null char and make separator idx out of bounds
        if(spaceidx < 0 && quoteidx < 0)
        {
            idx = -1;
            return '\0';
        }

        //If a space is found and no quote is found, then the separator is the space and set the separator idx to be the space index
        if(spaceidx > 0 && quoteidx < 0)
        {
            idx = spaceidx;
            return ' ';
        }

        //If a quote is found and no space is found, then the separator is the quote and set the separator idx to be the quote index
        if(quoteidx > 0 && spaceidx < 0)
        {
            idx = quoteidx;
            return quote;
        }

        idx = Mathf.Min(spaceidx, quoteidx);
        return idx == spaceidx ? ' ' : quote;
    }

    //Old way of doing it. Originally intended to allow string literals to be inside "" OR '', but now going to only support "" so that ' can be allowed inside a string literal
    /*private static string normalizestringlitsymbols(string str)
    {
        int i = indexofunescaped(str, '\'');

        //If no unescaped ' found, then return the same string
        if(i < 0)
            return str;

        while(i >= 0 && i < str.Length)
        {
            i = indexofunescaped(str, '\'', i);

            if(i > 0)
                str = replaceat(str, i, stringlitsymbol);

            i++;
        }

        return str;
    }*/

    private static string replaceat(string str, int index, char c)
    {
        char[] chars = str.ToCharArray();
        chars[index] = c;
        return new string(chars);
    }

    private static string substr(string str, int start, int end)
    {
        return (start >= 0 && end > start) ? str.Substring(start, end - start) : "";
    }

    private static int indexofquote(string str, int start = 0)
    {
        //Old
        //int i = start;

        //while(i >= 0 && i < str.Length)
        //{
        //    i = str.IndexOf(quoteSymbol, i);

        //    //If the quote was not escaped (or it occurs at the start of the string and cannot be escaped)
        //    if(i == 0 || i > 0 && str[i - 1] != '\\')
        //        return i;

        //    //Increment at the end before checking loop condition in case the previously found char was the last char in string (because always need to search from startIndex + 1 but cannot do
        //    //that if already on the last char in string)
        //    if(i > 0) i++;
        //}

        //return -1;

        return str.IndexOf(quote, start);
    }

    /// <summary>
    /// Extracts a string literal given in a command. This is required to be called from the start of an escaped string literal, i.e. if the command is print \"hi\" white then need to call this method
    /// with a start value of 7
    /// print \"hi\" white
    ///        ^
    ///        7
    /// </summary>
    /// <param name="str"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    private static string extractstringlit(string str, ref int start)
    {
        int end = indexofquote(str, start + 1);
        //Escape characters DO NOT CONSUME AN INDEX AND DO NOT CONTRIBUTE TO STRING LENGTH!!!!!!!!

        //Add one to the start to exclude the staring quote from the extracted string literal
        string s = substr(str, start + 1, end);

        start = end + 1;

        return s;
    }

    private static int parseargint(string argstr, out int argint)
    {
        if(int.TryParse(argstr, out argint))
            return 1;

        return 0;
    }

    /* /// <summary>
    /// Not finished/tested, just an idea. This is so you can pass in tuples as arguments like the color for print could be print hi (255,255,255)
    /// </summary>
    /// <param name="argstr"></param>
    /// <param name="argtuple"></param>
    /// <returns></returns>
    public static int parsetuple(string argstr, out string[] argtuple)
    {
        argstr = argstr.Trim();
        if(!(argstr[0].Equals('(') && argstr[argstr.Length - 1].Equals(')')))
        {
            argtuple = null;
            exec($"print 'invalid tuple: {argstr}'");
            return 0;
        }

        argstr = argstr.Replace(' ', '\0');
        argtuple = argstr.Split(',');
        return 0;
    }*/

    /// <summary>
    /// Shifts a string array (usually the command history string array) TOWARDS ZERO BY ONE INDEX. For example { "a", "b", "c", "d" } shifted becomes { "b", "c", "d", "" }
    /// </summary>
    /// <param name="arr"></param>
    private static void shiftarr(ref string[] arr)
    {
        for(int i = 0; i < arr.Length; i++)
            if(i > 0)
                arr[i - 1] = arr[i];
    }

    //commands
    private static void give()
    {

    }


    private static void tel()
    {

    }


    private static void kill()
    {

    }


    private static void log()
    {

    }


    private static void err()
    {

    }


    private static string manprint = "print [message] [color=null]";
    private static void print(string str, Color? color = null)
    {
        //The first child of the history stack is the line 'prefab' so just duplicate it to add a new line. The VerticalLayoutGroup on the historystack takes care of arranging the new line
        Transform line = Instantiate(instance.logstack.GetChild(0), instance.logstack);
        Text text = line.GetComponent<Text>();
        text.text = str;
        text.color = color != null ? (Color)color : white;

        //Scroll to newly printed line
        instance.consolelog.verticalNormalizedPosition = 0;
    }
}
