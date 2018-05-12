using System;
using System.Collections.Generic;
using System.Text;

namespace forward_chaining
{
    public class Rule
    {
        public char Right { get; set; }
        public char[] Left { get; set; }

        public Rule(char[] chars)
        {
            Left = new char[chars.Length - 1];
            Array.Copy(chars, 1, Left, 0, chars.Length - 1);
            Right = chars[0];
        }
    }
}
