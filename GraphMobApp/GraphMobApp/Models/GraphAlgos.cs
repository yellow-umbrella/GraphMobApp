using System;
using System.Collections.Generic;
using System.Text;

namespace GraphMobApp.Models
{
    internal class GraphAlgos
    {
        static public List<List<int>> FindPaths(int startVertex, int finishVertex, List<List<int>> graph)
        {
            List<List<int>> result = new List<List<int>>();
            FindPaths(startVertex, finishVertex, new List<int>(), result, graph);
            return result;
        }

        static private void FindPaths(int currentVertex, int finishVertex,
            List<int> path, List<List<int>> result, List<List<int>> graph)
        {
            path.Add(currentVertex);
            if (currentVertex == finishVertex)
            {
                result.Add(path);
                return;
            }
            foreach (var vertex in graph[currentVertex])
            {
                FindPaths(vertex, finishVertex, new List<int>(path), result, graph);
            }
        }

        /// <summary>
        /// Returns true if there are no cycles in graph
        /// </summary>
        static public bool CheckForCycles(List<List<int>> graph)
        {
            int n = graph.Count;
            int[] colors = new int[n];
            for (int i = 0; i < n; i++)
            {
                if (colors[i] == 0)
                {
                    if (!DFS(i, colors, graph))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static private bool DFS(int currentVertex, int[] colors, List<List<int>> graph)
        {
            colors[currentVertex] = 1;
            foreach (var vertex in graph[currentVertex])
            {
                if (colors[vertex] == 1)
                {
                    return false;
                }
                if (colors[vertex] == 0)
                {
                    if (!DFS(vertex, colors, graph))
                    {
                        return false;
                    }
                }
            }
            colors[currentVertex] = 2;
            return true;
        }

    }

}
