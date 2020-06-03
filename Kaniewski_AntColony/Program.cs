using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;



/*
[Sztuczna Inteligencja LAB]
Damian Kaniewski - 291565 - IS IIIr.
Problem Komiwojażera: Algorytm Mrówkowy
*/
namespace AntColony
{
    static class Defined
    {
        public const int nodeAmount = 6;
        public const int numAnts =  3;
        public const int time = 1000;
        public const float increase = 2.0f; // pheromone increase factor
        public const float evaporation = 0.01f; // pheromone decrease factor
    }
    internal class AntColonyKomiwojazer
    {

        private static Random random = new Random(0);

        // impact of pheromone on direction
        private static int alpha = 3;
        // impact of neighboring node distance
        private static int beta = 1;        
        private static double evaporation = Defined.evaporation;        
        private static double increase = Defined.increase;

        public static void Main(string[] args)
        {
                Console.WriteLine();
                Console.WriteLine("=============================================");
                Console.WriteLine("\tAntColony Optimization Algorithm");
                Console.WriteLine("=============================================");
                int numNodes = Defined.nodeAmount;
                int numAnts = Defined.numAnts;
                int maxTime = Defined.time;

                Console.WriteLine("Count of Nodes = " + numNodes);
                Console.WriteLine("Count of Ants = " + numAnts);
                Console.WriteLine("Time = " + maxTime);

                Console.WriteLine("Evaporation of pheromones = " + evaporation.ToString("F2"));
                Console.WriteLine("Increase of pheromones = " + increase.ToString("F2"));
                Console.WriteLine("\nImpact of pheromones = " + alpha);
                Console.WriteLine("Impact of neighboring node distance  = " + beta);
                
                Console.WriteLine();
                int[][] distance = InitializeGraph();                
                int[][] ants = InitAnts(numAnts, numNodes);
                                
                ShowAnts(ants, distance);

                int[] bestTrail = BestTrail(ants, distance);
                // determine the best initial trail
                double bestLength = DistanceOfTrail(bestTrail, distance);
                // the length of the best trail

                Console.Write("\nBest initial trail length: " + bestLength.ToString("F1") + "\n");
                Show(bestTrail);

                Console.WriteLine("\nInitializing pheromones on trails");
                double[][] pheromones = InitPheromones(numNodes);

                int time = 0;
               
                while (time < maxTime)
                {
                    UpdateAnts(ants, pheromones, distance);
                    UpdatePheromones(pheromones, ants, distance);

                    int[] currBestTrail = BestTrail(ants, distance);
                    double currBestLength = DistanceOfTrail(currBestTrail, distance);
                    if (currBestLength < bestLength)
                    {
                        bestLength = currBestLength;
                        bestTrail = currBestTrail;
                        Console.WriteLine("New best path of " + bestLength.ToString("F1") + " found at time " + time);
                    }
                    else if(time == maxTime-1)
                    {
                        Console.WriteLine("No better path found");
                    }
                    time += 1;
                }
                                
                Console.WriteLine("\nBest trail found: " + bestLength.ToString("F1"));
                Show(bestTrail);               
                Console.ReadLine();       
            
        }      

        private static int[][] InitAnts(int numAnts, int numNode)
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++)
            {
                int start = random.Next(0, numNode);
                ants[k] = TakeRandomTrail(start, numNode);
            }
            return ants;
        }
        private static int[] TakeRandomTrail(int start, int numNode)
        {
            int[] trail = new int[numNode];
                        
            for (int i = 0; i <= numNode - 1; i++)
            {
                trail[i] = i;
            }

            
            for (int i = 0; i <= numNode - 1; i++) // Fisher-Yates shuffle
            {
                int x = random.Next(i, numNode);
                int tmp = trail[x];
                trail[x] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexHelper(trail, start);            
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private static int IndexHelper(int[] trail, int target)
        {
            // helper for TakeRandomTrail
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception();
        }

        private static double DistanceOfTrail(int[] trail, int[][] dists)
        {
            double result = 0.0;
            double x = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], dists);
                x = Distance(trail[i+1], trail[0], dists);
            }                       
            return result + x;
        }
        private static double Distance(int X, int Y, int[][] dists)
        {
            return dists[X][Y];
        }
        private static int[] BestTrail(int[][] ants, int[][] dists)
        {
            
            double bestLength = DistanceOfTrail(ants[0], dists);
            int idxBestLength = 0;
            for (int k = 1; k <= ants.Length - 1; k++)
            {
                double len = DistanceOfTrail(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numNodes = ants[0].Length;
            int[] bestTrail_Renamed = new int[numNodes];
            ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }
        private static double[][] InitPheromones(int numNode)
        {   //information about pheromones
            double[][] pheromones = new double[numNode][];

            for (int i = 0; i <= numNode - 1; i++)
            {
                pheromones[i] = new double[numNode];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                }
            }
            return pheromones;
        }
        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            int numNode = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                int start = random.Next(0, numNode);
                int[] newTrail = MakeTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }
        private static int[] MakeTrail(int k, int startNode, double[][] pheromones, int[][] dists)
        {
            int numNodes = pheromones.Length;
            int[] trail = new int[numNodes];
            bool[] visited = new bool[numNodes];
            trail[0] = startNode;
            visited[startNode] = true;
            for (int i = 0; i <= numNodes - 2; i++)
            {
                int X = trail[i];
                int next = NextNode_Roulette(k, X, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }
        private static int NextNode_Roulette(int k, int X, bool[] visited, double[][] pheromones, int[][] dists)
        {            
            double[] probs = MoveProbs(k, X, visited, pheromones, dists);
            double[] sumOf = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                sumOf[i + 1] = sumOf[i] + probs[i];               
            }
            sumOf[sumOf.Length - 1] = 1.00;
            double p = random.NextDouble();

            for (int i = 0; i <= sumOf.Length - 2; i++)
            {
                if (p >= sumOf[i] && p < sumOf[i + 1])
                {
                    return i;
                }
            }
            return 0;
        }
        private static double[] MoveProbs(int k, int X, bool[] visited, double[][] pheromones, int[][] dists)
        {
            int numNodes = pheromones.Length;
            double[] pheromoneFactor = new double[numNodes];
            double sum = 0.0; 

          
            for (int i = 0; i <= pheromoneFactor.Length - 1; i++)
            {
                if (i == X)
                {
                    pheromoneFactor[i] = 0.0;                    
                }
                else if (visited[i] == true)
                {
                    pheromoneFactor[i] = 0.0;                    
                }
                else
                {
                    pheromoneFactor[i] = Math.Pow(pheromones[X][i], alpha) * Math.Pow((1.0 / Distance(X, i, dists)), beta);
                    
                    if (pheromoneFactor[i] < 0.0001)
                    {
                        pheromoneFactor[i] = 0.0001;
                    }
                    else if (pheromoneFactor[i] > (double.MaxValue / (numNodes * 100)))
                    {
                        pheromoneFactor[i] = double.MaxValue / (numNodes * 100);
                    }
                }
                sum += pheromoneFactor[i];
            }

            double[] probs = new double[numNodes];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = pheromoneFactor[i] / sum;                
            }
            return probs;
        }
        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++)
                {
                    for (int k = 0; k <= ants.Length - 1; k++)
                    {
                        double length = DistanceOfTrail(ants[k], dists);                       
                        double decrease = (1.0 - evaporation) * pheromones[i][j];
                        double increase = 0.0;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = AntColonyKomiwojazer.increase / length;
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001)
                        {
                            pheromones[i][j] = 0.0001;
                        }
                        else if (pheromones[i][j] > 100000.0)
                        {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }
        private static bool EdgeInTrail(int X, int Y, int[] trail) //NodeX and NodeY
        {
            int lastIndex = trail.Length - 1;
            int idx = IndexHelper(trail, X);

            if (idx == 0 && trail[1] == Y)
            {                return true;            }
            else if (idx == 0 && trail[lastIndex] == Y)
            {                return true;            }
            else if (idx == 0)
            {                return false;            }
            else if (idx == lastIndex && trail[lastIndex - 1] == Y)
            {                return true;            }
            else if (idx == lastIndex && trail[0] == Y)
            {                return true;            }
            else if (idx == lastIndex)
            {                return false;            }
            else if (trail[idx - 1] == Y)
            {                return true;            }
            else if (trail[idx + 1] == Y)
            {                return true;            }
            else
            {                return false;           }
        }



        private static int[][] InitializeGraph()
        {           
            int[][] graph1 = new int[][]
            {
                new int[] { 0, 2, 3, 5},
                new int[] { 2, 0, 6, 1},
                new int[] { 3, 6, 0, 7},
                new int[] { 5, 1, 7, 0},
            };

            int [][] graph2 = new int[][]
            { 
            new int[] { 0, 2, 3, 2, 1, 5 },
            new int[] { 2, 0, 6, 2, 5, 1 },
            new int[] { 3, 6, 0, 3, 2, 7 },
            new int[] { 2, 2, 3, 0, 5, 1 },
            new int[] { 1, 5, 2, 5, 0, 9 },
            new int[] { 5, 1, 7, 1, 9, 0 },
            };                      
            return graph2;
        }

       


        private static void Show(int[] trail)
        {
            Console.Write("[");
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                Console.Write(trail[i] + " ");
                if (i > 0 && i % 20 == 0)
                {                    Console.WriteLine("");                }
            }
            Console.WriteLine(trail[0]+ "]");           
        }
        private static void ShowAnts(int[][] ants, int[][] dists)
        {
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                Console.Write(i + ": [ ");

                for (int j = 0; j <= Defined.nodeAmount-1 ; j++)
                {
                    Console.Write(ants[i][j] + " ");
                    if(j==Defined.nodeAmount-1)
                    {
                        Console.Write(ants[i][j-(Defined.nodeAmount - 1)] + " ");
                    }
                }              
               
                Console.Write("] len = ");
                double len = DistanceOfTrail(ants[i], dists);
                Console.Write(len.ToString("F1"));
                Console.WriteLine("");
            }
        }

        private static void Display(double[][] pheromones)
        {
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                Console.Write(i + ": ");
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    Console.Write(pheromones[i][j].ToString("F4").PadLeft(8) + " ");
                }
                Console.WriteLine("");
            }
        }
    }   
}

