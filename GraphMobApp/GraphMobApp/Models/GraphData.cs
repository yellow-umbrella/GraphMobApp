using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Xamarin.Forms;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GraphMobApp.Models
{
    class GraphData
    {
        public List<List<int>> Graph { get; set; }
        public List<List<int>> Paths { get; set; }
        
        private int _vertexCount;
        public int VertexCount 
        {
            get => _vertexCount;
            set 
            {
                if (value >= 2 && value <= 10)
                {
                    _vertexCount = value;
                    if (value > Graph.Count)
                    {
                        for (int i = Graph.Count; i < value; i++)
                        {
                            Graph.Add(new List<int>());
                        }
                    } else if (value < Graph.Count) 
                    {
                        Graph.RemoveRange(value, Graph.Count - value);
                        foreach (var list in Graph)
                        {
                            list.RemoveAll(x => x >= value);
                        }
                    }
                }
            } 
        }
        public int StartVertex { get; set; }
        public int FinishVertex { get; set; }

        public GraphData() 
        {
            Graph = new List<List<int>>();
            Paths = new List<List<int>>();

            VertexCount = 0;
            StartVertex = 0;
            FinishVertex = 0;
        }

        public GraphData(int vertexCount)
        {
            Graph = new List<List<int>>();
            Paths = new List<List<int>>();

            VertexCount = vertexCount;
            StartVertex = 0;
            FinishVertex = 0;
        }

        public void Save(string path)
        {
            string data = JsonConvert.SerializeObject(this, Formatting.Indented);
            Debug.WriteLine(data);
            File.WriteAllText(path, data);
        }

        public static GraphData Load(string path)
        {
            GraphData graphData = new GraphData();
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                graphData.Graph = new List<List<int>>();
                graphData = JsonConvert.DeserializeObject<GraphData>(data);
            }
            return graphData;
        }
    }
}
