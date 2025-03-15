using UnityEngine;

public interface ITab
{
    void OnTabSelected();
    void OnTabDeselected();
    bool IsInitialized { get; }
    string TabName { get; }
}