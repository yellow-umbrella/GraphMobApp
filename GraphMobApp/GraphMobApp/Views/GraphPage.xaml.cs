using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using GraphMobApp.Models;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using Newtonsoft.Json;

namespace GraphMobApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GraphPage : ContentPage
    {
        string fileName = Path.Combine(Environment.
            GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "graph.txt");
        CheckBox[,] checkBoxes;
        GraphData graphData;

        SKCanvas canvas;
        SKImageInfo canvasInfo;

        bool canShowPath = false;

        public GraphPage()
        {
            InitializeComponent();
            //graphData = GraphData.Load(fileName);
            graphData = new GraphData(2);
            CreateGraphGrid();
        }

        void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            graphData.VertexCount = (int)e.NewValue;
            CreateGraphGrid();
        }

        void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (graphData != null)
            {
                FindPaths();
                graphData.Save(fileName);
            }
        }

        void OnLoadButtonClicked(object sender, EventArgs e)
        {
            graphData = GraphData.Load(fileName);
            stepperVertexCount.Value = graphData.VertexCount;
            stepperStartVertex.Value = graphData.StartVertex;
            stepperFinishVertex.Value = graphData.FinishVertex;
            CreateGraphGrid();
        }

        void OnFindPathsButtonClicked(object sender, EventArgs e)
        {
            FindPaths();
        }

        private void FindPaths()
        {
            ConvertGraph();
            if (GraphAlgos.CheckForCycles(graphData.Graph))
            {
                graphData.StartVertex = (int)stepperStartVertex.Value - 1;
                graphData.FinishVertex = (int)stepperFinishVertex.Value - 1;
                graphData.Paths = GraphAlgos.FindPaths(graphData.StartVertex,
                    graphData.FinishVertex, graphData.Graph);
                canShowPath = true;
            }
        }

        private void ConvertGraph()
        {
            graphData.Graph = new List<List<int>>();
            for (int i = 0; i < graphData.VertexCount; i++)
            {
                graphData.Graph.Add(new List<int>());
                for (int j = 0; j < graphData.VertexCount; j++)
                {
                    if (checkBoxes[i, j].IsChecked)
                    {
                        graphData.Graph[i].Add(j);
                    }
                }
            }
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            canvasInfo = args.Info;
            var surface = args.Surface;
            canvas = surface.Canvas;

            DrawGraph();
        }

        private void DrawGraph()
        {
            canvas.Clear();
            Point center = new Point(canvasInfo.Width / 2, canvasInfo.Height / 2);
            double radius = 0.45 * Math.Min(canvasInfo.Width, canvasInfo.Height);

            Point[] points = new Point[graphData.VertexCount]; 

            for (int i = 0; i < graphData.VertexCount; i++)
            {
                double size = 5;
                double radians = i * 2 * Math.PI / graphData.VertexCount;
                points[i].X = center.X + radius * Math.Sin(radians) - size / 2;
                points[i].Y = center.Y - radius * Math.Cos(radians) - size / 2;
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.Red.ToSKColor(), // Alternatively: SKColors.Red
                };
                canvas.DrawCircle((float)points[i].X, (float)points[i].Y, (float)size, paint);
            }

            ConvertGraph();
            for (int i = 0; i < graphData.VertexCount;i++)
            {

            }
        }

        public void CreateGraphGrid()
        {
            int n = graphData.VertexCount;

            checkBoxes = new CheckBox[n, n];
            graphGrid.Children.Clear();
            graphGrid.ColumnDefinitions.Clear();
            graphGrid.RowDefinitions.Clear();

            // create grid of checkboxes
            for (int i = 0; i <= n; i++)
            {
                graphGrid.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0)
                    {
                        var column = new ColumnDefinition();
                        column.Width = GridLength.Star;
                        graphGrid.ColumnDefinitions.Add(column);
                        if (j != 0)
                        {
                            var label = new Label
                            {
                                Text = j.ToString(),
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Center
                            };
                            graphGrid.Children.Add(label, j, i);
                        }
                    } else if (j == 0)
                    {
                        var label = new Label
                        {
                            Text = i.ToString(),
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };
                        graphGrid.Children.Add(label, j, i);
                    } else
                    {
                        checkBoxes[i-1, j-1] = new CheckBox { IsChecked = false };
                        graphGrid.Children.Add(checkBoxes[i-1, j-1], j, i);
                    }

                }
            }
            
            // fill checkboxes
            for (int i = 0; i < n; i++)
            {
                foreach (int v in graphData.Graph[i])
                {
                    checkBoxes[i, v].IsChecked = true;
                }
            }
        }
    }
}