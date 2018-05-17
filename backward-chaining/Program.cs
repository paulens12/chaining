using forward_chaining;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace backward_chaining
{
    class Program
    {
        static StreamWriter writer;
        static List<Rule> rules = new List<Rule>();
        static bool[] flag1;
        static bool[] flag2;
        static List<int> rulesUsed = new List<int>();
        static List<char> facts = new List<char>();
        static List<char> earlierInferred = new List<char>();
        static Stack<char> goals = new Stack<char>();

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
                Console.ReadKey();
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
            Write("\n  3) Goal\n     {0}\n\n", goals.Peek());

            Write("PART 2. Trace\n\n");

            flag1 = new bool[rules.Count];
            flag2 = new bool[rules.Count];
            for(int i=0; i<rules.Count; i++)
            {
                flag1[i] = flag2[i] = false;
            }

            char goal = goals.Peek();

            Write("PART 3. Results\n");
            if(facts.Contains(goal))
            {
                Write("  Goal {0} in facts. Empty path.");
            }
            else if(Solve(0))
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

        private static bool Solve(int step)
        {
            StringBuilder sb = new StringBuilder();
            // initial fact?
            if(facts.Contains(goals.Peek()))
            {
                for (int i = 0; i < step; i++)
                    sb.Append("-");
                sb.AppendFormat("Goal {0}. Fact (initial), as facts are {1}. Back, OK.\n", goals.Peek(), String.Join(" and ", String.Join(", ", facts), String.Join(", ", earlierInferred)));
                Write(sb.ToString());
                goals.Pop();
                return true;
            }
            
            // earlier inferred?
            if(earlierInferred.Contains(goals.Peek()))
            {
                for (int i = 0; i < step; i++)
                    sb.Append("-");
                sb.AppendFormat("Goal {0}. Fact (earlier inferred), as facts are {1}. Back, OK.\n", goals.Peek(), String.Join(" and ", String.Join(", ", facts), String.Join(", ", earlierInferred)));
                Write(sb.ToString());
                goals.Pop();
                return true;
            }

            bool found = false;
            bool success;
            // find rules for recursion
            foreach (int rule in FindRules(goals.Peek()))
            {
                success = true;
                found = true;
                sb.Clear();
                for (int i = 0; i < step; i++)
                    sb.Append("-");
                sb.AppendFormat("Goal {0}. Find R{1}:{2}->{3}. New goals {4}.\n", goals.Peek(), rule+1, String.Join(",", rules[rule].Left), rules[rule].Right, String.Join(", ", rules[rule].Left));
                Write(sb.ToString());
                foreach(char newGoal in rules[rule].Left)
                {
                    goals.Push(newGoal);
                    if (!Solve(step + 1))
                    {
                        // this rule is invalid
                        success = false;
                        rulesUsed.Clear();
                        break;
                    }
                }
                if(success)
                {
                    sb.Clear();
                    for (int i = 0; i < step; i++)
                        sb.Append("-");
                    earlierInferred.Add(goals.Peek());
                    sb.AppendFormat("Goal {0}. Fact (presently inferred). Facts {1}. Back, OK.\n", goals.Pop(), String.Join(" and ", String.Join(", ", facts), String.Join(", ", earlierInferred)));
                    Write(sb.ToString());
                    rulesUsed.Add(rule);
                    //goals.Pop();
                    return true;
                }
            }

            sb.Clear();
            for (int i = 0; i < step; i++)
                sb.Append("-");
            sb.AppendFormat("Goal {0}. No {1}rules. Back, FAIL.\n", goals.Pop(), found ? "more " : "");
            Write(sb.ToString());
            return false;
        }

        private static List<int> FindRules(char goal)
        {
            var answer = new List<int>();

            for(int i=0; i<rules.Count; i++)
                if (rules[i].Right == goal)
                    answer.Add(i);

            return answer;
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
            while (!lines[index].StartsWith("1) Rules"))
                index++;

            // only 2+ space delimited single letters allowed
            while (String.IsNullOrEmpty(lines[index + 1]) || Regex.IsMatch(lines[index + 1], "^(([A-Z])\\s)+[A-Z]$"))
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

            while (String.IsNullOrEmpty(lines[index + 1]) || Regex.IsMatch(lines[index + 1], "^(([A-Z])\\s)*[A-Z]$"))
            {
                index++;
                if (String.IsNullOrEmpty(lines[index]))
                    continue;
                // parse and save rule
                char[] line = (from l in lines[index].Split(' ') select l[0]).ToArray();
                foreach (char c in line)
                {
                    facts.Add(c);
                    //earlierInferred.Add(c);
                }
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
                goals.Push(lines[index][0]);
                break;
            }
        }
    }
}
