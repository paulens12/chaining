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
            writer.Flush();
            #if DEBUG
                Console.ReadKey();
            #endif
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
