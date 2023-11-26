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
using System.Numerics;
using System.Drawing;

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
        int activePath = -1;

        public GraphPage()
        {
            InitializeComponent();
            graphData = new GraphData(2);
            CreateGraphGrid();
            Update();
        }

        void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            graphData.VertexCount = (int)e.NewValue;
            CreateGraphGrid();
        }
        void OnStartValueChanged(object sender, ValueChangedEventArgs e)
        {
            graphData.StartVertex = (int)stepperStartVertex.Value - 1;
            Update();
        }

        void OnFinishValueChanged(object sender, ValueChangedEventArgs e)
        {
            graphData.FinishVertex = (int)stepperFinishVertex.Value - 1;
            Update();
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
            stepperStartVertex.Value = graphData.StartVertex + 1;
            stepperFinishVertex.Value = graphData.FinishVertex + 1;
            CreateGraphGrid();
            Update();
        }

        void OnFindPathsButtonClicked(object sender, EventArgs e)
        {
            if (graphData.Paths.Count > 0)
            {
                activePath = (activePath + 1) % graphData.Paths.Count;
            }
            CanvasView.InvalidateSurface();
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

        private void Update()
        {
            ConvertGraph();
            canShowPath = GraphAlgos.CheckForCycles(graphData.Graph); 
            if (!canShowPath)
            {
                pathButton.IsEnabled = false;
                activePath = -1;
            } else
            {
                pathButton.IsEnabled = true;
                FindPaths();
                if (graphData.Paths.Count > 0)
                {
                    activePath = 0;
                } else
                {
                    activePath = -1;
                }
            }
            CanvasView.InvalidateSurface();
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
            SKPoint center = new SKPoint(canvasInfo.Width / 2, canvasInfo.Height / 2);
            float radius = 0.45f * (float)Math.Min(canvasInfo.Width, canvasInfo.Height);
            SKPoint[] points = new SKPoint[graphData.VertexCount]; 
            SKPaint paint;
            float size = 20;

            // find positions
            for (int i = 0; i < graphData.VertexCount; i++)
            {
                double radians = i * 2 * Math.PI / graphData.VertexCount;
                points[i].X = center.X + radius * (float)Math.Sin(radians) - size / 2;
                points[i].Y = center.Y - radius * (float)Math.Cos(radians) - size / 2;
            }

            // draw edges
            paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 3,
            };
            for (int i = 0; i < graphData.VertexCount;i++)
            {
                foreach (int v in graphData.Graph[i])
                {
                    DrawLine(points[i], points[v], size, paint, canvas);
                }
            }
        
            // show path
            if (canShowPath && activePath > -1)
            {
                var path = graphData.Paths[activePath];
                paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.CornflowerBlue,
                    StrokeWidth = 6,
                };
                canvas.DrawCircle(points[path[0]], size, paint);
                for (int i = 1; i < path.Count; i++)
                {
                    DrawLine(points[path[i - 1]], points[path[i]], size, paint, canvas);
                    canvas.DrawCircle(points[path[i]], size, paint);
                }

            }
            
            // draw vertices
            for (int i = 0; i < graphData.VertexCount; i++)
            {
                paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.DeepPink,
                };
                canvas.DrawCircle(points[i], size, paint);

                var textPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = size * 2,
                };
                string str = (i + 1).ToString();
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(str, ref textBounds);
                float xText = points[i].X - textBounds.MidX;
                float yText = points[i].Y - textBounds.MidY;
                canvas.DrawText(str, xText, yText, textPaint);
            }
        }

        private void DrawLine(SKPoint start, SKPoint finish, float offset, SKPaint paint, SKCanvas canvas)
        {
            Vector2 vS = new Vector2(start.X, start.Y);
            Vector2 vF = new Vector2(finish.X, finish.Y);
            Vector2 vLine = vF - vS;
            Vector2 vOf = vLine / vLine.Length() * offset;
            vS += vOf;
            vF -= vOf;
            canvas.DrawLine(vS.X, vS.Y, vF.X, vF.Y, paint);

            float angle = 0.5f;
            Vector2 vAr1 = new Vector2(vOf.X * (float)Math.Cos(angle) - vOf.Y * (float)Math.Sin(angle),
                                       vOf.X * (float)Math.Sin(angle) + vOf.Y * (float)Math.Cos(angle));
            Vector2 vAr2 = new Vector2(vOf.X * (float)Math.Cos(-angle) - vOf.Y * (float)Math.Sin(-angle),
                                       vOf.X * (float)Math.Sin(-angle) + vOf.Y * (float)Math.Cos(-angle));
            vAr1 = vF - vAr1;
            vAr2 = vF - vAr2;
            canvas.DrawLine(vAr1.X, vAr1.Y, vF.X, vF.Y, paint);
            canvas.DrawLine(vAr2.X, vAr2.Y, vF.X, vF.Y, paint);
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

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    checkBoxes[i, j].CheckedChanged += (sender, e) => { Update(); };
                }
            }
        }
    }
}