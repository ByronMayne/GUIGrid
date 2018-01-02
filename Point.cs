using UnityEngine;
using System;

[Serializable]
public struct Point
{
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Gets the value at an index.
    /// </summary>
    /// <param name="index">The index you are trying to get.</param>
    /// <returns>The value at that index.</returns>
    public int this[int index]
    {
        get
        {
            int result;
            if (index != 0)
            {
                if (index != 1)
                {
                    throw new IndexOutOfRangeException("Index " + index.ToString() + " is out of range.");
                }
                result = y;
            }
            else
            {
                result = x;
            }
            return result;
        }
        set
        {
            if (index != 0)
            {
                if (index != 1)
                {
                    throw new IndexOutOfRangeException("Index " + index.ToString() + " is out of range.");
                }
                y = value;
            }
            else
            {
                x = value;
            }
        }
    }

    /// <summary>
    /// Sets the x and y components of an existing Point
    /// </summary>
    /// <param name="x">The new x value</param>
    /// <param name="y">The new y value</param>
    public void Set(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Clamps a point to a minimum value for both x and y and 
    /// returns the result. 
    /// </summary>
    public Point ClampMin(int min)
    {
        Point point = this;
        if (point.x < min) x = min;
        if (point.y < min) y = min;
        return point;
    }

    /// <summary>
    /// Clamps a point to a maximum value for both x and y and 
    /// returns the result. 
    /// </summary>
    public Point ClampMax(int max)
    {
        Point point = this;
        if (point.x > max) x = max;
        if (point.y > max) y = max;
        return point;
    }

    /// <summary>
    /// Clamps the min and max values of both x and y to a value and
    /// returns the result.
    /// </summary>
    public void Clamp(int min, int max)
    {
        x = Mathf.Clamp(x, min, max);
        y = Mathf.Clamp(y, min, max);
    }

    /// <summary>
    /// Shorthand for writing new Point(0,0).
    /// </summary>
    public static Point zero
    {
        get
        {
            return new Point(0, 0);
        }
    }

    /// <summary>
    /// Shorthand for writing new Point(1,1).
    /// </summary>
    public static Point one
    {
        get
        {
            return new Point(1, 1);
        }
    }

    public static explicit operator Vector2(Point point)
    {
        return new Vector2((float)point.x, (float)point.y);
    }

    public static explicit operator Point(Vector2 vector2)
    {
        return new Point((int)vector2.x, (int)vector2.y);
    }

    public static Point operator +(Point lhs, Point rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        return lhs;
    }

    public static Point operator -(Point lhs, Point rhs)
    {
        lhs.x -= rhs.x;
        lhs.y -= rhs.y;
        return lhs;
    }

    public static Point operator *(Point lhs, Point rhs)
    {
        lhs.x *= rhs.x;
        lhs.y *= rhs.y;
        return lhs;
    }

    public static Point operator /(Point lhs, Point rhs)
    {
        lhs.x /= rhs.x;
        lhs.y /= rhs.y;
        return lhs;
    }

    public static bool operator ==(Point lhs, Point rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.x;
    }

    public static bool operator !=(Point lhs, Point rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.x;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = (int)2166136261;
            hash = (hash * 16777619) ^ x;
            hash = (hash * 16777619) ^ y;
            return hash;
        }
    }

    public override bool Equals(object other)
    {
        if (!(other is Point))
        {
            return false;
        }

        Point point = (Point)other;
        return x == point.x && y == point.y;
    }

    public override string ToString()
    {
        return string.Join(", ", new string[] { x.ToString(), y.ToString() });
    }
}