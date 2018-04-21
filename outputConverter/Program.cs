using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace outputConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Path> result = parsePaths("res.txt", false);
            WriteOutput("pathFinal", result);
        }

        /// <summary>
        /// Parse paths from the output of MAPF algorithm.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static List<Path> parsePaths(string fileName, bool contract)
        {
            StreamReader sr = new StreamReader(fileName);
            List<Path> result = new List<Path>();
            string line;
            bool validSolution = false;

            while (!sr.EndOfStream)
            {
                result.Clear();

                // comments between tables
                while (!((line = sr.ReadLine()).StartsWith("|")))
                {
                    line = sr.ReadLine();
                }

                // top border of the table
                sr.ReadLine();

                // rows for parsing
                while ((line = sr.ReadLine()).StartsWith("|"))
                {
                    List<NodeL> newSteps = parseCoordinates(line);

                    // initializing path variables
                    if (result.Count == 0)
                    {
                        for (int i = 0; i < newSteps.Count; i++)
                        {
                            result.Add(new Path());                                
                        }
                    }

                    // add new step to every path
                    for (int i = 0; i < newSteps.Count; i++)
                    {
                        // middle-cell (not visible in our maps)
                        if (contract && (newSteps[i].x % 2 == 1 || newSteps[i].y % 2 == 1))
                        {
                            continue;
                        }
                        else // true steps - recount of coordinates
                        {
                            if (contract)
                            {
                                newSteps[i].x /= 2;
                                newSteps[i].y /= 2;
                            }
                            result[i].path.Add(newSteps[i]);
                        }
                    }
                }

                line = sr.ReadLine();
                if (line.StartsWith("A valid execution!"))
                {
                    validSolution = true;
                    break;
                }
            }

            if (validSolution)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        static List<NodeL> parseCoordinates(string line)
        {
            string[] parts = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] textCord = new string[parts.Length - 1];
            for (int i = 0; i < textCord.Length; i++)
            {
                textCord[i] = parts[i+1].Trim();
            }

            List<NodeL> steps = new List<NodeL>();
            for (int i = 0; i < textCord.Length; i++)
            {
                parts = textCord[i].Split(',');
                steps.Add(new NodeL(int.Parse(parts[0]), int.Parse(parts[1])));
            }

            return steps;
        }

        /// <summary>
        /// Writes output in format for ozocode generator.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="result"></param>
        static void WriteOutput(string file, List<Path> result)
        {
            int id = 0;

            foreach (Path p in result)
            {
                StreamWriter sw = new StreamWriter(file + "-" + id + ".txt");
                //View v = View.detectView(p.path[0], p.path[1]);
                View v = new RightView();

                for (int i = 0; i < p.path.Count - 1; i++)
                {
                    sw.WriteLine((int)v.translateMove(p.path[i], p.path[i + 1], ref v));
                }

                sw.Close();
                id++;
            }
        }
    }
    /// <summary>
    /// Child classes represent possible view on the grid (like ozobot). The name of the child specifies the direction ozobot is looking from the beginning.
    /// </summary>
    class View
    {
        public static View detectView(NodeL from, NodeL to)
        {
            int first = to.x - from.x;
            int second = to.y - from.y;

            if (first > 0)
            {
                return new DownView();
            }
            else if (first < 0)
            {
                return new TopView();
            }
            else if (second > 0)
            {
                return new RightView();
            }
            else
            {
                return new LeftView();
            }
        }

        public virtual DIRECTION translateMove(NodeL from, NodeL to, ref View v)
        {
            return 0;
        }

    }

    class LeftView : View
    {
        public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
        {
            int first = to.x - from.x;
            int second = to.y - from.y;

            if (first > 0) // dolu
            {
                v = new DownView();
                return DIRECTION.DIRECTION_LEFT;
            }
            else if (first < 0) // nahoru
            {
                v = new TopView();
                return DIRECTION.DIRECTION_RIGHT;
            }
            else if (second > 0) // doprava
            {
                v = new RightView();
                return DIRECTION.DIRECTION_BACKWARD;
            }
            else if (second < 0) // doleva
            {
                return DIRECTION.DIRECTION_FORWARD;
            }
            else
            {
                return DIRECTION.WAIT;
            }
        }
    }

    class RightView : View
    {
        public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
        {
            int first = to.x - from.x;
            int second = to.y - from.y;

            if (first > 0) // dolu
            {
                v = new DownView();
                return DIRECTION.DIRECTION_RIGHT;
            }
            else if (first < 0) // nahoru
            {
                v = new TopView();
                return DIRECTION.DIRECTION_LEFT;
            }
            else if (second > 0) // doprava
            {
                return DIRECTION.DIRECTION_FORWARD;
            }
            else if (second < 0) // doleva
            {
                v = new LeftView();
                return DIRECTION.DIRECTION_BACKWARD;
            }
            else
            {
                return DIRECTION.WAIT;
            }
        }
    }

    class TopView : View
    {
        public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
        {
            int first = to.x - from.x;
            int second = to.y - from.y;

            if (first > 0) // dolu
            {
                v = new DownView();
                return DIRECTION.DIRECTION_BACKWARD;
            }
            else if (first < 0) // nahoru
            {
                return DIRECTION.DIRECTION_FORWARD;
            }
            else if (second > 0) // doprava
            {
                v = new RightView();
                return DIRECTION.DIRECTION_RIGHT;
            }
            else if (second < 0)// doleva
            {
                v = new LeftView();
                return DIRECTION.DIRECTION_LEFT;
            }
            else
            {
                return DIRECTION.WAIT;
            }
        }
    }

    class DownView : View
    {
        public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
        {
            int first = to.x - from.x;
            int second = to.y - from.y;

            if (first > 0) // dolu
            {
                return DIRECTION.DIRECTION_FORWARD;
            }
            else if (first < 0) // nahoru
            {
                v = new TopView();
                return DIRECTION.DIRECTION_BACKWARD;
            }
            else if (second > 0) // doprava
            {
                v = new RightView();
                return DIRECTION.DIRECTION_LEFT;
            }
            else if (second < 0) // doleva
            {
                v = new LeftView();
                return DIRECTION.DIRECTION_RIGHT;
            }
            else
            {
                return DIRECTION.WAIT;
            }
        }

    }

    enum DIRECTION
    {
        DIRECTION_LEFT = 2, DIRECTION_RIGHT = 4, DIRECTION_FORWARD = 1, DIRECTION_BACKWARD = 8, WAIT = 0
    }

    class NodeL
    {
        public NodeL(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y;
    }

    /// <summary>
    /// Path for agent a found by LowLevelSearch
    /// </summary>
    class Path
    {
        public Path()
        {
            this.path = new List<NodeL>();
        }

        public int GetCost()
        {
            return this.path.Count();
        }

        //Agent a;
        public List<NodeL> path;
        //public int cost;
    }
}
