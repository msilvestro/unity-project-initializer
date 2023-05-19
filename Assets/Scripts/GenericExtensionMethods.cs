using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pawnball
{
    public static class GenericExtensionMethods
    {
        public static bool ContainsLayer(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static bool BelongsToLayerMask(this Collider2D collider, LayerMask layerMask)
        {
            return layerMask.ContainsLayer(collider.gameObject.layer);
        }

        /// <summary>
        /// Create a rotation so that the object looks at the given direction.
        /// Only for 2D games.
        /// </summary>
        public static Quaternion LookRotation2D(this Vector2 direction)
        {
            // https://forum.unity.com/threads/look-rotation-2d-equivalent.611044/#post-4092259
            return Quaternion.LookRotation(Vector3.forward, direction);
        }

        /// <summary>
        /// Apply a rotation of the given angle in a 2D space.
        /// Only for 2D games.
        /// </summary>
        public static Quaternion Rotate2D(this Quaternion quaternion, float angle)
        {
            return quaternion * Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// Rotate the given rotation vertically (on the y axis).
        /// </summary>
        public static Quaternion FlipVertically2D(this Quaternion rotation)
        {
            Vector3 euler = rotation.eulerAngles;
            euler.z = 180 - euler.z;
            return Quaternion.Euler(euler);
        }

        /// <summary>
        /// Integrate function f(x) using the trapezoidal rule between xLow and
        /// xHigh.
        /// </summary>
        private static float Integrate(Func<float, float> f, float xLow, float xHigh, int nSteps)
        {
            float h = (xHigh - xLow) / nSteps;
            float res = (f(xLow) + f(xHigh)) / 2;
            for (int i = 1; i < nSteps; i++)
            {
                res += f(xLow + i * h);
            }
            return h * res;
        }

        /// <summary>
        /// Integrate area under AnimationCurve between start and end time.
        /// </summary>
        public static float Integrate(
            this AnimationCurve curve,
            float startTime,
            float endTime,
            int steps
        )
        {
            return Integrate(curve.Evaluate, startTime, endTime, steps);
        }

        public static Vector2 AngleToVector2(this float angle)
        {
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        public static Vector2 Rotate(this Vector2 vector, float angle)
        {
            return new Vector2(
                vector.x * Mathf.Cos(angle * Mathf.Deg2Rad)
                    - vector.y * Mathf.Sin(angle * Mathf.Deg2Rad),
                vector.x * Mathf.Sin(angle * Mathf.Deg2Rad)
                    + vector.y * Mathf.Cos(angle * Mathf.Deg2Rad)
            );
        }

        public static string Capitalize(this string word)
        {
            return word[..1].ToUpper() + word[1..].ToLower();
        }
        
        public static string ToPascalCase(this string sentence)
        {
            var words = sentence.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var tailWords = words
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();
            return string.Join(string.Empty, tailWords);
        }

        public static string[] SplitByNewLine(this string str)
        {
            char[] newLines = new[] { '\r', '\n' };
            return str.Split(newLines, StringSplitOptions.RemoveEmptyEntries);
        }

        public static Transform DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
            return transform;
        }

        public static string ToString(this Vector2 vector)
        {
            return $"({vector.x}, {vector.y})";
        }

        public static bool BelongsToSegment(
            this Vector2 point,
            Vector2 segmentA,
            Vector2 segmentB,
            float tolerance = 0
        )
        {
            float AB = Vector2.Distance(segmentA, segmentB);
            float AP = Vector2.Distance(segmentA, point);
            float PB = Vector2.Distance(point, segmentB);
            return Mathf.Abs(AP + PB - AB) < tolerance;
        }

        public static float FinalVelocityToForceMagnitude(this Rigidbody2D rb, float finalVelocity)
        {
            return rb.mass * finalVelocity * Mathf.Clamp01(rb.drag * Time.fixedDeltaTime);
        }

        public static void DrawDebugCrosshair(this Vector2 point, float wideness = 0.1f)
        {
            Debug.DrawLine(
                point + new Vector2(-1, 0) * wideness,
                point + new Vector2(1, 0) * wideness
            );
            Debug.DrawLine(
                point + new Vector2(0, -1) * wideness,
                point + new Vector2(0, 1) * wideness
            );
        }

        public static (Vector2, float) Difference(this Vector3 origin, Vector2 target)
        {
            Vector2 difference = target - (Vector2)origin;
            return (difference.normalized, difference.magnitude);
        }

        public static (Vector2, float) Difference(this Vector2 origin, Vector2 target)
        {
            return ((Vector3)origin).Difference(target);
        }

        public static T[] FindGameObjectsWithTag<T>(this GameObject gameObject, string tag)
        {
            return GameObject
                .FindGameObjectsWithTag(tag)
                .Select((o => o.GetComponent<T>()))
                .Where((movement => movement != null))
                .ToArray();
        }

        public static Collider2D GetEnabledCollider2D(this GameObject gameObject)
        {
            Collider2D[] components = gameObject.GetComponents<Collider2D>();
            foreach (Collider2D component in components)
            {
                if (component.enabled)
                {
                    return component;
                }
            }
            return null;
        }

        public static T GetEnabledComponent<T>(this GameObject gameObject)
            where T : Behaviour
        {
            T[] components = gameObject.GetComponents<T>();
            foreach (T component in components)
            {
                if (component.enabled)
                {
                    return component;
                }
            }
            return null;
        }

        public static bool IsColliding(this Collider2D collider, Collider2D other)
        {
            return collider.bounds.Intersects(other.bounds);
        }

        public static float Round(this float number, int decimalPlaces = 0)
        {
            float decimalPower = Mathf.Pow(10, decimalPlaces);
            return Mathf.Round(number * decimalPower) / decimalPower;
        }
    }
}
