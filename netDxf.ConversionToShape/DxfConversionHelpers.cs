using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace netDxf.ConversionToShape
{
    public static class DxfConversionHelpers
    {
        internal static void ConvertDxfEntityToGeometries(DxfDocument document, EntityObject entity, IList<Geometry> geometries)
        {
            switch (entity)
            {
                case Entities.Line line:
                    {
                        LineGeometry drawLine = CreateLineGeometry(line);
                        geometries.Add(drawLine);
                        break;
                    }

                case Entities.Circle circle:
                    {
                        EllipseGeometry drawCircle = CreateCircleGeometry(circle);
                        geometries.Add(drawCircle);
                        break;
                    }


                case Entities.Ellipse ellipse:
                    {
                        Geometry drawEllipse = CreateEllipseGeometry(ellipse);
                        geometries.Add(drawEllipse);
                        break;
                    }

                case Entities.Arc arc:
                    {
                        var arcPath = CreateArcPathGeometry(arc);
                        geometries.Add(arcPath);
                        break;
                    }

                case Entities.Polyline3D polyLine:
                    {
                        var lines = CreatePolyLineGeometry(polyLine);
                        lines.ForEach(line => geometries.Add(line));
                        break;
                    }

                case Entities.Polyline2D lwPolyLine:
                    {
                        var lines = CreateLWPolyLineGeometry(lwPolyLine);
                        lines.ForEach(x => geometries.Add(x));
                        break;
                    }

                case Entities.Solid solid:
                    {
                        PathGeometry path = CreateSolidGeometry(solid);
                        geometries.Add(path);
                        break;
                    }

                case Entities.Insert insert:
                    {
                        Blocks.Block block = document.Blocks.FirstOrDefault(x => x.Name == insert.Block.Name);
                        if (block != null && block.HasChildren)// && !block.is
                        {
                            IList<Geometry> blockEntities = new List<Geometry>();
                            foreach (EntityObject blockEntity in block.Entities)
                                ConvertDxfEntityToGeometries(document, blockEntity, blockEntities);

                            double centerX = insert.Position.X;
                            double centerY = -insert.Position.Y;

                            foreach (Geometry geometry in blockEntities)
                            {
                                TranslateTransform translateTransform = new TranslateTransform(centerX, centerY);
                                RotateTransform rotateTransform = null;
                                ScaleTransform scaleTransform = null;

                                rotateTransform = new RotateTransform(insert.Rotation > 180 ? insert.Rotation - 180 : insert.Rotation, centerX, centerY);
                                scaleTransform = new ScaleTransform(insert.Scale.X, insert.Scale.Y, centerX, centerY);

                                // Create a TransformGroup to contain the transforms 
                                TransformGroup transformGroup = new TransformGroup();
                                transformGroup.Children.Add(translateTransform);
                                if (rotateTransform != null)
                                    transformGroup.Children.Add(rotateTransform);
                                if (scaleTransform != null)
                                    transformGroup.Children.Add(scaleTransform);

                                geometry.Transform = transformGroup;
                                geometries.Add(geometry);
                            }
                        }

                        break;
                    }


            }
        }

        internal static void ConvertDxfEntityToShapes(DxfDocument document, EntityObject entity, IList<System.Windows.Shapes.Shape> shapes, Brush stroke)
        {
            switch (entity)
            {
                case Entities.Line line:
                    {
                        System.Windows.Shapes.Line drawLine = CreateLine(stroke, line);
                        shapes.Add(drawLine);
                        break;
                    }

                case Entities.Circle circle:
                    {
                        System.Windows.Shapes.Ellipse drawCircle = CreateCircle(stroke, circle);
                        shapes.Add(drawCircle);
                        break;
                    }

                case Entities.Ellipse ellipse:
                    {
                        System.Windows.Shapes.Path drawEllipse = CreateEllipse(stroke, ellipse);
                        shapes.Add(drawEllipse);
                        break;
                    }

                case Entities.Arc arc:
                    {
                        var path = CreateArcPath(stroke, arc);
                        shapes.Add(path);
                        break;
                    }

                case Entities.Polyline3D polyLine:
                    {
                        List<System.Windows.Shapes.Shape> lines = CreatePolyLine(stroke, polyLine);
                        lines.ForEach(x => shapes.Add(x));
                        break;
                    }

                case Entities.Polyline2D lwPolyLine:
                    {
                        List<System.Windows.Shapes.Shape> lines = CreateLWPolyLine(stroke, lwPolyLine);
                        lines.ForEach(x => shapes.Add(x));
                        break;
                    }

                case Entities.Solid solid:
                    {
                        System.Windows.Shapes.Path path = CreateSolid(stroke, solid);
                        shapes.Add(path);
                        break;
                    }

                case Entities.Insert insert:
                    {
                        Blocks.Block block = document.Blocks.FirstOrDefault(x => x.Name == insert.Block.Name);
                        if (block != null && block.HasChildren)//&& !block.IsInvisible
                        {
                            SolidColorBrush brush = new SolidColorBrush(Colors.Red);
                            brush.Freeze();

                            IList<System.Windows.Shapes.Shape> blockEntities = new List<System.Windows.Shapes.Shape>();
                            foreach (EntityObject blockEntity in block.Entities)
                                ConvertDxfEntityToShapes(document, blockEntity, blockEntities, brush);

                            double centerX = insert.Position.X;
                            double centerY = -insert.Position.Y;

                            foreach (System.Windows.Shapes.Shape shape in blockEntities)
                            {
                                TranslateTransform translateTransform = new TranslateTransform(centerX, centerY);
                                RotateTransform rotateTransform = null;
                                ScaleTransform scaleTransform = null;

                                rotateTransform = new RotateTransform(insert.Rotation > 180 ? insert.Rotation - 180 : insert.Rotation, centerX, centerY);
                                scaleTransform = new ScaleTransform(insert.Scale.X, insert.Scale.Y, centerX, centerY);

                                // Create a TransformGroup to contain the transforms 
                                TransformGroup transformGroup = new TransformGroup();
                                transformGroup.Children.Add(translateTransform);
                                if (rotateTransform != null)
                                    transformGroup.Children.Add(rotateTransform);
                                if (scaleTransform != null)
                                    transformGroup.Children.Add(scaleTransform);

                                shape.RenderTransform = transformGroup;
                                shapes.Add(shape);
                            }
                        }

                        break;
                    }

                case Entities.Text text:
                    {
                        break;
                    }

                case Entities.MText _:
                    break;
                case Entities.Dimension _:
                    break;
                case Entities.Hatch _:
                    break;
                case Entities.Face3D _:
                    break;
                case Entities.Trace _:
                    break;
                case Entities.Spline _:
                    break;
                case Entities.Point _:
                    break;
                case Entities.XLine _:
                    break;
                case Entities.Viewport _:
                    break;
                case Entities.Image _:
                    break;

                default:
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    break;
            }
        }

        internal static Path CreateSolid(Brush stroke, Entities.Solid solid)
        {
            Path path = new Path();
            PathGeometry geometry = CreateSolidGeometry(solid);
            path.Data = geometry;
            path.Fill = stroke;
            path.StrokeThickness = 0;
            return path;
        }

        internal static PathGeometry CreateSolidGeometry(Entities.Solid solid)
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            PathSegmentCollection group = new PathSegmentCollection
            {
                //reverse 3, 2 because ordering of vertices is different in WPF
                new LineSegment(new System.Windows.Point(solid.SecondVertex.X, -solid.SecondVertex.Y), true),
                new LineSegment(new System.Windows.Point(solid.FourthVertex.X, -solid.FourthVertex.Y), true),
                new LineSegment(new System.Windows.Point(solid.ThirdVertex.X, -solid.ThirdVertex.Y), true),
                new LineSegment(new System.Windows.Point(solid.FirstVertex.X, -solid.FirstVertex.Y), true)
            };

            figure.IsFilled = true;
            figure.StartPoint = new System.Windows.Point(solid.FirstVertex.X, -solid.FirstVertex.Y);
            figure.Segments = group;

            geometry.Figures.Add(figure);

            return geometry;
        }

        internal static Path CreateArcPath(Brush stroke, Entities.Arc arc)
        {
            Path path = new Path();
            path.Stroke = stroke;

            PathGeometry geometry = CreateArcPathGeometry(arc);
            path.Data = geometry;
            path.IsHitTestVisible = false;

            return path;
        }

        private static PathGeometry CreateArcPathGeometry(Entities.Arc arc)
        {
            var endPoint = new System.Windows.Point(
                (arc.Center.X + Math.Cos(arc.EndAngle * Math.PI / 180) * arc.Radius),
                (-arc.Center.Y - Math.Sin(arc.EndAngle * Math.PI / 180) * arc.Radius));

            var startPoint = new System.Windows.Point(
                (arc.Center.X + Math.Cos(arc.StartAngle * Math.PI / 180) * arc.Radius),
                (-arc.Center.Y - Math.Sin(arc.StartAngle * Math.PI / 180) * arc.Radius));

            ArcSegment arcSegment = new ArcSegment();
            double sweep;
            if (arc.EndAngle < arc.StartAngle)
                sweep = (360 + arc.EndAngle) - arc.StartAngle;
            else sweep = Math.Abs(arc.EndAngle - arc.StartAngle);

            arcSegment.IsLargeArc = sweep >= 180;
            arcSegment.Point = endPoint;
            arcSegment.Size = new Size(arc.Radius, arc.Radius);
            arcSegment.SweepDirection = arc.Normal.Z >= 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

            PathGeometry geometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = startPoint;
            pathFigure.Segments.Add(arcSegment);
            geometry.Figures.Add(pathFigure);
            return geometry;
        }

        internal static System.Windows.Shapes.Ellipse CreateCircle(Brush stroke, Entities.Circle circle)
        {
            System.Windows.Shapes.Ellipse drawCircle = new System.Windows.Shapes.Ellipse
            {
                Stroke = stroke,
                Width = circle.Radius * 2,
                Height = circle.Radius * 2,
                IsHitTestVisible = false
            };

            System.Windows.Point center = new System.Windows.Point(circle.Center.X, circle.Center.Y);
            var left = (center.X - circle.Radius);
            var top = (-center.Y - circle.Radius);
            Canvas.SetLeft(drawCircle, left);
            Canvas.SetTop(drawCircle, top);

            return drawCircle;
        }

        internal static EllipseGeometry CreateCircleGeometry(Entities.Circle circle)
        {
            var center = new System.Windows.Point((double)circle.Center.X, (double)circle.Center.Y);
            EllipseGeometry drawCircle = new EllipseGeometry
            {
                RadiusX = circle.Radius,
                RadiusY = circle.Radius,
                Center = center
            };

            return drawCircle;
        }

        internal static Path CreateEllipse(Brush stroke, Entities.Ellipse ellipse)
        {
            Path path = new Path();
            path.Stroke = stroke;
            path.IsHitTestVisible = false;

            var angle = (180 / Math.PI) * Math.Atan2(-ellipse.MinorAxis, ellipse.MajorAxis);
            path.RenderTransform = new RotateTransform(angle, ellipse.Center.X, -ellipse.Center.Y);

            Geometry geometry = CreateEllipseGeometry(ellipse);
            path.Data = geometry;

            return path;
        }

        private static Geometry CreateEllipseGeometry(Entities.Ellipse ellipse)
        {
            Geometry geometry;
            var radiusX = Math.Sqrt(Math.Pow(ellipse.MajorAxis, 2) + Math.Pow(ellipse.MinorAxis, 2));
            var radiusY = radiusX * ellipse.AxisRatio;
            var startAngle = ellipse.StartAngle * 180 / Math.PI;
            var endAngle = ellipse.EndAngle * 180 / Math.PI;

            if (endAngle - startAngle < 360)
            {
                var endPoint = new System.Windows.Point(
                    (ellipse.Center.X + Math.Cos(ellipse.StartAngle) * radiusX),
                    (-ellipse.Center.Y - Math.Sin(ellipse.EndAngle) * radiusY));

                var startPoint = new System.Windows.Point(
                    (ellipse.Center.X + Math.Cos(ellipse.StartAngle) * radiusX),
                    (-ellipse.Center.Y - Math.Sin(ellipse.StartAngle) * radiusY));

                ArcSegment arcSegment = new ArcSegment();
                double sweep;
                if (endAngle < startAngle)
                    sweep = (360 + endAngle) - startAngle;
                else sweep = Math.Abs(endAngle - startAngle);

                arcSegment.IsLargeArc = sweep >= 180;
                arcSegment.Point = endPoint;
                arcSegment.Size = new Size(radiusX, radiusY);
                arcSegment.SweepDirection = ellipse.Normal.Z >= 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = startPoint;
                pathFigure.Segments.Add(arcSegment);
                pathGeometry.Figures.Add(pathFigure);
                geometry = pathGeometry;
            }
            else
            {
                EllipseGeometry ellipseGeometry = new EllipseGeometry();
                ellipseGeometry.Center = new System.Windows.Point(ellipse.Center.X, -ellipse.Center.Y);
                ellipseGeometry.RadiusX = Math.Sqrt(Math.Pow(ellipse.MajorAxis, 2) + Math.Pow(ellipse.MinorAxis, 2));
                ellipseGeometry.RadiusY = ellipseGeometry.RadiusX * ellipse.MinorAxis / ellipse.MajorAxis;
                geometry = ellipseGeometry;
            }

            return geometry;
        }

        internal static System.Windows.Shapes.Line CreateLine(Brush stroke, Entities.Line line)
        {
            var start = new System.Windows.Point((float)line.StartPoint.X, (float)-line.StartPoint.Y);
            var end = new System.Windows.Point((float)line.EndPoint.X, (float)-line.EndPoint.Y);

            var drawLine = new System.Windows.Shapes.Line
            {
                Stroke = stroke,
                X1 = end.X,
                X2 = start.X,
                Y1 = end.Y,
                Y2 = start.Y,
                IsHitTestVisible = false
            };

            return drawLine;
        }

        internal static LineGeometry CreateLineGeometry(Entities.Line line)
        {
            var start = new System.Windows.Point((float)line.StartPoint.X, (float)-line.StartPoint.Y);
            var end = new System.Windows.Point((float)line.EndPoint.X, (float)-line.EndPoint.Y);

            LineGeometry drawLine = new LineGeometry
            {
                StartPoint = start,
                EndPoint = end,
            };

            return drawLine;
        }

        internal static List<System.Windows.Shapes.Shape> CreateLWPolyLine(Brush stroke, Polyline2D polyLine)
        {
            bool isClosed = polyLine.IsClosed;

            int count = isClosed ? polyLine.Vertexes.Count : polyLine.Vertexes.Count - 1;
            List<System.Windows.Shapes.Shape> lines = new List<System.Windows.Shapes.Shape>();
            for (int i = 1; i <= count; i++)
            {
                Vector2 vertex1 = (i == polyLine.Vertexes.Count) ? polyLine.Vertexes[0].Position : polyLine.Vertexes[i].Position;
                Vector2 vertex2 = polyLine.Vertexes[i - 1].Position;

                // TODO: Handle Element.Bulge http://www.afralisp.net/archive/lisp/Bulges1.htm

                System.Windows.Point start = new System.Windows.Point(vertex1.X, -vertex1.Y);
                System.Windows.Point end = new System.Windows.Point(vertex2.X, -vertex2.Y);

                System.Windows.Shapes.Line drawLine = new System.Windows.Shapes.Line
                {
                    Stroke = stroke,
                    X1 = end.X,
                    X2 = start.X,
                    Y1 = end.Y,
                    Y2 = start.Y,
                    IsHitTestVisible = false
                };

                lines.Add(drawLine);
            }

            return lines;
        }

        internal static List<LineGeometry> CreateLWPolyLineGeometry(Entities.Polyline2D polyLine)
        {
            bool isClosed = polyLine.IsClosed;

            int count = isClosed ? polyLine.Vertexes.Count : polyLine.Vertexes.Count - 1;
            List<LineGeometry> lines = new List<LineGeometry>();
            for (int i = 1; i <= count; i++)
            {
                Vector2 vertex1 = (i == polyLine.Vertexes.Count) ? polyLine.Vertexes[0].Position : polyLine.Vertexes[i].Position;
                Vector2 vertex2 = polyLine.Vertexes[i - 1].Position;

                // TODO: Handle Element.Bulge http://www.afralisp.net/archive/lisp/Bulges1.htm

                var start = new System.Windows.Point(vertex1.X, -vertex1.Y);
                var end = new System.Windows.Point(vertex2.X, -vertex2.Y);

                LineGeometry drawLine = new LineGeometry
                {
                    StartPoint = start,
                    EndPoint = end,
                };

                lines.Add(drawLine);
            }

            return lines;
        }

        internal static List<System.Windows.Shapes.Shape> CreatePolyLine(Brush stroke, Entities.Polyline3D polyLine)
        {
            bool isClosed = polyLine.IsClosed;

            int count = isClosed ? polyLine.Vertexes.Count : polyLine.Vertexes.Count - 1;
            List<System.Windows.Shapes.Shape> lines = new List<System.Windows.Shapes.Shape>();
            for (int i = 1; i <= count; i++)
            {
                Vector3 vertex1 = (i == polyLine.Vertexes.Count) ? polyLine.Vertexes[0] : polyLine.Vertexes[i];
                Vector3 vertex2 = polyLine.Vertexes[i - 1];

                System.Windows.Point start = new System.Windows.Point(vertex1.X,-vertex1.Y);
                System.Windows.Point end = new System.Windows.Point(vertex2.X, -vertex2.Y);

                // TODO: Handle Vertex.Buldge http://www.afralisp.net/archive/lisp/Bulges1.htm

                System.Windows.Shapes.Line drawLine = new System.Windows.Shapes.Line
                {
                    Stroke = stroke,
                    X1 = end.X,
                    X2 = start.X,
                    Y1 = end.Y,
                    Y2 = start.Y,
                    IsHitTestVisible = false
                };

                lines.Add(drawLine);
            }

            return lines;
        }

        internal static List<LineGeometry> CreatePolyLineGeometry(Entities.Polyline3D polyLine)
        {
            bool isClosed = polyLine.IsClosed;

            int count = isClosed ? polyLine.Vertexes.Count : polyLine.Vertexes.Count - 1;
            List<LineGeometry> lines = new List<LineGeometry>();
            for (int i = 1; i <= count; i++)
            {
                Vector3 vertex1 = (i == polyLine.Vertexes.Count) ? (Vector3)polyLine.Vertexes[0] : (Vector3)polyLine.Vertexes[i];
                Vector3 vertex2 = (Vector3)polyLine.Vertexes[i - 1];

                var start = new System.Windows.Point(vertex1.X, -vertex1.Y);
                var end = new System.Windows.Point(vertex2.X, -vertex2.Y);

                // TODO: Handle Vertex.Buldge http://www.afralisp.net/archive/lisp/Bulges1.htm

                LineGeometry drawLine = new LineGeometry
                {
                    StartPoint = start,
                    EndPoint = end,
                };

                lines.Add(drawLine);
            }

            return lines;
        }

        public static (DxfDocument Document, IList<System.Windows.Shapes.Shape> Shapes) LoadDxf(string fileName)
        {
            if (File.Exists(fileName))
            {
                // Parse DXF
                DxfDocument document = ReadDXF(fileName);

                if (document.Entities.All.Count() > 0)
                {
                    // Create shapes
                    DateTime start = DateTime.UtcNow;

                    IList<System.Windows.Shapes.Shape> shapes = new List<System.Windows.Shapes.Shape>();

                    SolidColorBrush brush = new SolidColorBrush(Colors.SteelBlue);
                    brush.Freeze();

                    foreach (var entity in document.Entities.All)
                        ConvertDxfEntityToShapes(document, entity, shapes, brush);

                    Debug.WriteLine("Created shapes in {0}ms", DateTime.UtcNow.Subtract(start).TotalMilliseconds);

                    return (document, shapes);
                }
                else if (Debugger.IsAttached)
                    Debugger.Break();
            }
            else if (Debugger.IsAttached)
                Debugger.Break();

            return (null, null);
        }

        public static (DxfDocument Document, Path Path) LoadDxfGeometries(string fileName)
        {
            if (File.Exists(fileName))
            {
                // Parse DXF
                DxfDocument document = ReadDXF(fileName);

                if (document.Entities.All.Count() > 0)
                {
                    // Create shapes
                    DateTime start = DateTime.UtcNow;

                    IList<Geometry> geometries = new List<Geometry>();

                    SolidColorBrush brush = new SolidColorBrush(Colors.SteelBlue);
                    brush.Freeze();

                    foreach (EntityObject entity in document.Entities.All)
                        ConvertDxfEntityToGeometries(document, entity, geometries);

                    Debug.WriteLine("Created geometries in {0}ms", DateTime.UtcNow.Subtract(start).TotalMilliseconds);

                    GeometryGroup group = new GeometryGroup();
                    foreach (var geometry in geometries)
                        group.Children.Add(geometry);

                    Path path = new Path() { Data = group, Stroke = brush };

                    return (document, path);
                }
                else if (Debugger.IsAttached)
                    Debugger.Break();
            }
            else if (Debugger.IsAttached)
                Debugger.Break();

            return (null, null);
        }

        public static DxfDocument ReadDXF(string fileName)
        {
            DateTime start = DateTime.UtcNow;
            DxfDocument document = DxfDocument.Load(fileName);
            Debug.WriteLine("Loaded {0} in {1}ms", System.IO.Path.GetFileName(fileName), DateTime.UtcNow.Subtract(start).TotalMilliseconds);
            return document;
        }

    }
}
