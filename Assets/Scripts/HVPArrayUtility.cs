using System;

public static class HVPArrayUtility {
    // Utility to prepend a value to an array
    public static T[] Prepend<T>(this T[] array, T value) {
        T[] copy = new T[array.Length + 1];
        copy[0] = value;
        Array.Copy(array, 0, copy, 1, array.Length);
        return copy;
    }
}
