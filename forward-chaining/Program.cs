using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace forward_chaining
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

        static int iteration = 1;
        static bool Solve()
        {
            if (initialFacts.Contains(goal))
            {
                Write("  Goal achieved.");
                return true;
            }
            while(!facts.Contains(goal))
            {
                Write("ITERATION {0}\n", iteration++);
                bool success = false;
                for(int i=0; i<rules.Count; i++)
                {
                    Write("    R{0}:{1}->{2} ", i + 1, String.Join(',', rules[i].Left), rules[i].Right);
                    if(flag1[i])
                    {
                        Write("skip, because flag1 raised.\n");
                        continue;
                    }
                    if(flag2[i])
                    {
                        Write("skip, because flag2 raised.\n");
                        continue;
                    }

                    List<char> missing = new List<char>();
                    foreach(char node in rules[i].Left)
                    {
                        if (!facts.Contains(node) && !initialFacts.Contains(node))
                            missing.Add(node);

                    }
                    // can be applied
                    if(missing.Count == 0)
                    {
                        if(facts.Contains(rules[i].Right) || initialFacts.Contains(rules[i].Right))
                        {
                            Write("not applied, because RHS is in facts. Raise flag2.\n");
                            flag2[i] = true;
                            continue;
                        }
                        else
                        {
                            facts.Add(rules[i].Right);
                            Write("apply. Raise flag1. Facts {0} and {1}.\n", String.Join(", ", initialFacts), String.Join(", ", facts));
                            flag1[i] = true;
                            rulesUsed.Add(i);
                            success = true;
                            if(rules[i].Right == goal)
                                Write("    Goal achieved.\n");
                            break;
                        }
                    }

                    //wasn't applied...
                    Write("not applied, because of lacking {0}.\n", String.Join(", ", missing));

                }
                Write("\n");
                if (!success) return false;
            }
            return true;
        }

        public static void Write(string format, params object[] arg)
        {
            #if DEBUG
                Console.Write(format, arg);
            #endif
                writer.Write(format, arg);
        }

        static void ReadFile(string file)
        {
            string[] input = File.ReadAllLines(file);
            
            // ignore comments
            var lines = (from l in input select Regex.Replace(l, "[/]{2}.+", "")).ToArray();

            // trim whitespace
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // find rules
            int index = 0;
            while(!lines[index].StartsWith("1) Rules"))
                index++;
            
            // only 2+ space delimited single letters allowed
            while(String.IsNullOrEmpty(lines[index+1]) || Regex.IsMatch(lines[index+1], "^(([A-Z])\\s)+[A-Z]$"))
            {
                index++;
                if (String.IsNullOrEmpty(lines[index]))
                    continue;
                // parse and save rule
                char[] line = (from l in lines[index].Split(' ') select l[0]).ToArray();
                rules.Add(new Rule(line));
            }
            
            // find facts
            while (!lines[index].StartsWith("2) Facts"))
                index++;

            while (String.IsNullOrEmpty(lines[index+1]) || Regex.IsMatch(lines[index+1], "^(([A-Z])\\s)*[A-Z]$"))
            {
                index++;
                if (String.IsNullOrEmpty(lines[index]))
                    continue;
                // parse and save rule
                char[] line = (from l in lines[index].Split(' ') select l[0]).ToArray();
                foreach (char c in line)
                    facts.Add(c);
            }

            // find goal
            while (!lines[index].StartsWith("3) Goal"))
                index++;
            
            while (String.IsNullOrEmpty(lines[index + 1]) || Regex.IsMatch(lines[index + 1], "^[A-Z]$"))
            {
                index++;
                if (String.IsNullOrEmpty(lines[index]))
                    continue;
                // save goal
                goal = lines[index][0];
                break;
            }
        }
        

    }
}
