using UnityEngine;

public interface IShatterable
{
    bool IsFrozen { get; }
    void Shatter();
}
