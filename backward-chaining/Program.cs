using forward_chaining;
using System;
using System.Collections.Generic;
using System.IO;

namespace backward_chaining
{
    class Program
    {
        static StreamWriter writer;
        static List<Rule> rules = new List<Rule>();
        static bool[] flag1;
        static bool[] flag2;
        static List<int> rulesUsed = new List<int>();
        static List<char> initialFacts;
        static List<char> facts = new List<char>();
        static char goal;
        public static void Write(string format, params object[] arg)
        {
            #if DEBUG
                Console.Write(format, arg);
            #endif
                writer.Write(format, arg);
        }
        static void Main(string[] args)
        {
            string fin, fout;
            try
            {
                fin = args[0];
                fout = args[1];
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Incorrect arguments.\n1. input file name\n2. output file name");
                return;
            }
            writer = new StreamWriter(fout);
            try
            {
                ReadFile(fin);
            }
            catch(IndexOutOfRangeException)
            {
                Console.WriteLine("ERROR: incorrect input format. Press any key");
                Console.ReadKey();
                return;
            }
            Write("PART 1. Data\n\n");
            Write("  1) Rules\n");
            for (int i = 0; i < rules.Count; i++)
            {
                Write("     R{0}: {1} -> {2}\n", i+1, String.Join(", ", rules[i].Left), rules[i].Right);
            }
            Write("\n  2) Facts\n     {0}\n", String.Join(", ", facts));
            Write("\n  3) Goal\n     {0}\n\n", goal);

            Write("PART 2. Trace\n\n");

            initialFacts = new List<char>(facts);
            facts.Clear();

            flag1 = new bool[rules.Count];
            flag2 = new bool[rules.Count];
            for(int i=0; i<rules.Count; i++)
            {
                flag1[i] = flag2[i] = false;
            }

            bool success = Solve();

            Write("PART 3. Results\n");
            if(initialFacts.Contains(goal))
            {
                Write("  Goal {0} in facts. Empty path.");
            }
            else if(success)
            {
                Write("  1) Goal {0} achieved.\n  2) Path: {1}.", goal, String.Join(", ", from r in rulesUsed select String.Format("R{0}", r+1)));
            }
            else
            {
                Write("  No path found.");
            }

            writer.Flush();
            #if DEBUG
                Console.ReadKey();
            #endif
        }
    }
}
