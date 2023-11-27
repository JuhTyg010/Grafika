using System.Numerics;
using SixLabors.ImageSharp.Drawing.Processing;
namespace Mandala;

public class MyDraw
{
  private const int DEVIDER = 400;
    static void DrawSymmetryLines(Image<Rgba32> image, Vector2 center, Rgba32 color, int symmetryOrder)
    {
        int length = 400;
        float angle = 2 * (float)Math.PI / symmetryOrder;
        for (int i = 0; i < symmetryOrder; i++)
        {
            Vector2 end = center + new Vector2(length *(float)Math.Cos(i * angle), length * (float) Math.Sin(i * angle));
            // Draw the line
            image.Mutate(x => x.DrawLine(color, 1, center,end));
        }
    }
    public static void DrawBackground(Image<Rgba32> image, params Rgba32[] colors)
    {
        if (colors.Length == 1)
        {
            image.Mutate(x => x.Fill(colors[0]));
        }
        else
        {
            float lenght = (float)Math.Sqrt(Math.Pow(image.Width, 2) + Math.Pow(image.Height, 2))/2;
            Rgba32 color = colors[0];
            int steps = smallestStep(colors[0], colors[1]);
            int thick = lenght/steps < 1 ? 1 : (int) Math.Ceiling(lenght/steps);
            Vector3 diff = Differ(colors[0], colors[1], steps);
            image.Mutate(x => x.Fill(colors[1]));
            for (float i = 0; i < lenght ; i+=thick-1) {
                DrawRing(image, i,i + thick,200, color, new Vector2((float)image.Width/2, (float)image.Height/2),true);
                color = AddRgba32(color, diff);
                if(color == colors[1]) break;
            }
        }
    }

    //create points on some circle part which can be connected to draw curve
    static Vector2[] CreateCircularLine(float radius, Vector2 center, float angle, float rotation, int precision)
    {
        Vector2[] points = new Vector2[precision];
        for (int i = 0; i < precision; i++)
        {
            points[i] = GetPointOnCircle(radius, angle, rotation, (float)i/(precision - 1), center);
        }

        return points;
    }


    //creates the points
    static Vector2[] CreateTriangle(float innerRadius, float outerRadius, float rotation,
        Vector2 center, float start, float end, float outer, int symmetryOrder) {
        float angle = 2 * (float)Math.PI / symmetryOrder;

            //easiest shape is triangle so we just start with that

            Vector2 innerPoint1 = GetPointOnCircle(innerRadius, angle, rotation, start, center);
            Vector2 innerPoint2 = GetPointOnCircle(innerRadius, angle, rotation, end, center);
            Vector2 outerPoint = GetPointOnCircle(outerRadius, angle, rotation, outer, center);
            return new []{ innerPoint1, outerPoint, innerPoint2 };

    }

    /// <summary>
    /// Draws a triangles with rotational symmetry
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="symmetryOrder"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="outer"></param>
    /// <param name="isFilled"></param>
    public static void DrawTriangles(Image<Rgba32> image, float  innerRadius, float outerRadius, int symmetryOrder,
        Rgba32 color, Vector2 center, float start, float end, float outer, bool isFilled = false) {
      int thickness = Math.Min(image.Width, image.Height) / DEVIDER;
        for (int i = 0; i < symmetryOrder; i++) {
            //easiest shape is triangle so we just start with that
            PointF[] points = VectToPointFs(CreateTriangle( innerRadius, outerRadius, i,  center, start,
                end, outer, symmetryOrder));

            // Draw a line with rotational symmetry
            if (isFilled) {
                image.Mutate(x => x.Fill(color, path => path.AddLines(points)));
            }
            else {
                image.Mutate(x => x.DrawLine(color, thickness, points));
            }
        }
    }


    /// <summary>
    /// Draws a circle
    /// </summary>
    /// <param name="image"></param>
    /// <param name="radius"></param>
    /// <param name="precision"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="isFilled"></param>
    public static void DrawCircle(Image<Rgba32> image, float radius, int precision, Rgba32 color, Vector2 center, bool isFilled=false) {
      if(isFilled)
        DrawCirclePart(image, 0, radius, 2*(float)Math.PI, 0, precision, color, center, isFilled );
      else {
        int thickness = Math.Min(image.Width, image.Height) / DEVIDER;
        image.Mutate(x => x.DrawLine(color, thickness, VectToPointFs(CreateCircularLine(radius, center, 2*(float)Math.PI, 0, precision))));
      }
    }

    /// <summary>
    /// Draws a circle part
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="precision"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="isFilled"></param>
    public static void DrawRing(Image<Rgba32> image, float innerRadius, float outerRadius, int precision, Rgba32 color,
        Vector2 center, bool isFilled=true) {
        DrawCirclePart(image, innerRadius, outerRadius, 2*(float)Math.PI, 0, precision, color, center, true );
    }

    /// <summary>
    /// Draws a circle part as a curve not working Properly yet
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="angle"></param>
    /// <param name="rotation"></param>
    /// <param name="precision"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="isFilled"></param>
    /// <returns></returns>
    public static float DrawCrescent(Image<Rgba32> image, float innerRadius, float angle, float rotation,
        int precision, Rgba32 color, Vector2 center, bool isFilled = false)
    {
        Vector2[] points = CreateCircularLine(innerRadius, center, angle, rotation, precision);
        float littleRadius = innerRadius * (float)Math.Sin(angle / 2);

        #region GetRotationOfSmallCircle  -->  distance float is in here
        //get to analytic geometry

        Vector2 pointA = GetPointOnCircle(innerRadius, angle, rotation, 0, center);
        Vector2 pointB = GetPointOnCircle(innerRadius, angle, rotation, .5f, center);

        Vector3 lineBC = new Vector3(1, 0,-pointB.X); //x,y,z are for a, b, c in  ax+by+c=0

        //distance from point A to line BC is (a*x+b*y+c)/sqrt(a^2+b^2) --> a=1, b=0, c=-pointB.X
        float distance = (float)Math.Abs(pointA.X + lineBC.Z)/(float)Math.Sqrt(1);

        #endregion

        float littleAngle = (float)Math.Asin(distance / littleRadius);

        switch (rotation*angle) {
            case var n when n < Math.PI/2:
                littleAngle = (float) Math.PI*2 - littleAngle;
                break;
            case var n when n < Math.PI:
                break;
            case var n when n < 3*Math.PI/2:
                littleAngle = (float)Math.PI - littleAngle;
                break;
            case var n when n < 2*Math.PI:
                littleAngle += (float)Math.PI;
                break;
        }
        float actualAngle = (float)Math.PI + angle / 2;

        Vector2[] points2 = CreateCircularLine(littleRadius, GetPointOnCircle(innerRadius, angle, rotation, 0.5f, center)
            , actualAngle, littleAngle/actualAngle  , precision);

        if (isFilled) {
            image.Mutate(x => x.Fill(color, path => path.AddLines(VectToPointFs(points.Concat(points2.Reverse()).Append(points[0]).ToArray()))));
        }
        else {
            image.Mutate(x => x.DrawLine(color, 1, VectToPointFs(points.Concat(points2.Reverse()).Append(points[0]).ToArray())));
        }
        return innerRadius + littleRadius;
    }

    /// <summary>
    /// Draws a circle part with rotational symmetry
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="symmetryOrder"></param>
    /// <param name="precision"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="isFilled"></param>
    /// <returns></returns>
    public static float DrawCrescents(Image<Rgba32> image, float innerRadius, float symmetryOrder,
        int precision, Rgba32 color, Vector2 center, float startPoint, float endPoint , bool isFilled = false)
    {
        float angle = 2 * (float)Math.PI / symmetryOrder;
        float diff = endPoint - startPoint;
        float outerRadius = innerRadius;
        for (int i = 0; i < symmetryOrder; i++)
        {
           outerRadius = DrawCrescent(image, innerRadius, angle * diff,(i+startPoint)/diff , precision, color, center, isFilled);
        }

        return outerRadius;
    }

    /// <summary>
    /// Draws a circle with rotational symmetry around the center
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="symmetryOrder"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="isFilled"></param>
    public static void DrawCircles(Image<Rgba32> image, float innerRadius, float outerRadius,  int symmetryOrder, Rgba32 color,
        Vector2 center, bool isFilled = false)
    {
        float cirRadius = (outerRadius - innerRadius) / 2;
        float angle = 2 * (float) Math.PI / symmetryOrder;
        for (int i = 0; i < symmetryOrder; i++)
        {
            DrawCircle(image, cirRadius, 48, color,
                GetPointOnCircle(cirRadius+innerRadius, angle, i, 0.5f, center), isFilled);
        }
    }

    /// <summary>
    /// Function to draw a part of a circle with a given inner and outer radius, angle and starting rotation
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="angle"></param>
    /// <param name="rotation"></param>
    /// <param name="precision"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="isFilled"></param>
    public static void DrawCirclePart(Image<Rgba32> image, float innerRadius, float outerRadius, float angle, float rotation,
        int precision, Rgba32 color, Vector2 center, bool isFilled = false)
    {
      int thickness = Math.Min(image.Width, image.Height) / DEVIDER;


        //Get points for outer and inner radius to make close shape must be one of the outer/inner part reversed
        Vector2[] points = CreateCircularLine(outerRadius, center, angle, rotation, precision)
            .Concat(CreateCircularLine(innerRadius, center, angle, rotation, precision).Reverse()).ToArray();

        if (isFilled) {
            image.Mutate(x => x.Fill(color, path => path.AddLines(VectToPointFs(points))));
        }
        else {
            image.Mutate(x => x.DrawLine(color, thickness, VectToPointFs(points.Concat(new Vector2[] {points[0]}).ToArray())));
        }
    }

    /// <summary>
    /// Draws a poligonial shape with rotational symmetry around the center
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="symmetryOrder"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="isFilled"></param>
    /// <exception cref="Exception"></exception>
    public static void DrawRoundedSquares(Image<Rgba32> image, float innerRadius, float outerRadius, int symmetryOrder,
        Rgba32 color, Vector2 center,float start, float end, bool isFilled = false)
    {
        float angle = 2 * (float) Math.PI / symmetryOrder;
        float diff = end - start;
        if (diff < 0) throw new Exception("Start is bigger than end");
        for (int i = 0; i < symmetryOrder; i++)
        {
            DrawCirclePart(image, innerRadius, outerRadius, angle * diff, (float)(i+start)/diff
                , 50, color, center, isFilled);
        }

    }

    /// <summary>
    /// Draws a poligonial shape with rotational symmetry around the center
    /// </summary>
    /// <param name="image"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="middleRadius"></param>
    /// <param name="symmetryOrder"></param>
    /// <param name="color"></param>
    /// <param name="center"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="isFilled"></param>
    /// <exception cref="Exception"></exception>
    public static void DrawDiamonds(Image<Rgba32> image, float innerRadius, float outerRadius, float middleRadius, int symmetryOrder,
        Rgba32 color, Vector2 center, float start, float end, bool isFilled = false)
    {
      int thickness = Math.Min(image.Width, image.Height) / DEVIDER;

        float angle = 2 * (float) Math.PI / symmetryOrder;
        float diff = end - start;
        if (diff < 0) throw new Exception("Start is bigger than end");
        for (int i = 0; i < symmetryOrder; i++)
        {
            Vector2 innerPoint = GetPointOnCircle(innerRadius, angle, i, 0.5f, center);
            Vector2 middlePoint1 = GetPointOnCircle(middleRadius, angle, i, start, center);
            Vector2 outerPoint = GetPointOnCircle(outerRadius, angle, i, 0.5f, center);
            Vector2 middlePoint2 = GetPointOnCircle(middleRadius, angle, i, end, center);
            PointF[] points = VectToPointFs(new []{ innerPoint, middlePoint1, outerPoint, middlePoint2, innerPoint });
            if (isFilled) {
                image.Mutate(x => x.Fill(color, path => path.AddLines(points)));
            }
            else {
                image.Mutate(x => x.DrawLine(color, thickness, points));
            }
        }
    }

    static void DrawSymmetric(Image<Rgba32> image, float innerRadius, float outerRadius, int symmetryOrder,
         Action<List<Object>> draw) {

    }

    static PointF VectToPointF(Vector2 from) {
        return new PointF(from.X, from.Y);
    }

    static PointF[] VectToPointFs(Vector2[] from) {
        PointF[] to = new PointF[from.Length];
        for(int i=0; i< from.Length; i++) {
            to[i] = VectToPointF(from[i]);
        }
        return to;
    }

    static Vector3 Differ(Rgba32 from, Rgba32 to, int stepCount)
    {
        float rDiff = to.R - from.R;
        float gDiff = to.G - from.G;
        float bDiff = to.B - from.B;

        return new Vector3(rDiff / stepCount, gDiff / stepCount, bDiff / stepCount);
    }


    static int smallestStep(Rgba32 from, Rgba32 to)
    {
      int smallest = 1;
      Vector3 diff;
      for(int i=400; i > 0; i--)
      {
        diff = Differ(from, to, i);
        Rgba32 color = from;
        for (int j = 0; j < i; j++)
        {
          color = AddRgba32(color, diff);
        }

        if (color == to)
          return i;
      }
      return smallest;
    }
    static Rgba32 AddRgba32(Rgba32 one, Vector3 two)
    {
        one.R += (byte)two.X;
        one.G += (byte)two.Y;
        one.B += (byte)two.Z;

        return one;
    }

    // same formula all over the place Vector2 where is some radius * cos(angle*i + part of angle) and sin(angle*i + part of angle)
    /// <param name="radius">radius is the radius of the circle</param>
    /// <param name="angle">angle is the angle between the x = radius y = 0 and the wanted point</param>
    /// <param name="rotation">rotation is angle multiplier to count</param>
    /// <param name="partOfAngle">partOfAngle is the part of the angle from 0 to 1</param>
    /// <param name="center">center is center of circle we are on</param>
    /// <returns></returns>
    static Vector2 GetPointOnCircle(float radius, float angle,float rotation, float partOfAngle, Vector2 center) {
        return center + new Vector2(radius * (float)Math.Cos(rotation * angle + partOfAngle * angle), radius *
            (float)Math.Sin(rotation * angle + partOfAngle * angle));
    }
}
