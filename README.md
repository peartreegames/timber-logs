## Timber Logs
Code from "Creating a Runtime Debug Logger in Unity" [YouTube video]()

## Installation

Can be installed via the Package Manager > Add Package From Git URL...
`https://github.com/peartreegames/timber-logs.git`

## Usage

On any MonoBehaviour: `this.Timber(string message)`

#### Example:

``` csharp
using UnityEngine;

public class MyClass : MonoBehaviour 
{
    public void Start() 
    {
        this.Timber("Starting");
    }
    
    public void OnDestroy()
    {
        this.Timber("Destroying");
    }
}
```

#### Output:

```csharp
[MyClass.Start] Starting
[MyClass.OnDestroy] Destroying
```

## Default Hotkeys

| Key              | Description                  |
|------------------|------------------------------|
| Ctrl+Shift+L     | Toggle Logs Search           |
| Ctrl+Shift+F     | Toggle Focused Logs Filter   |
| Ctrl+Shift+W     | Close Focused Logs           |
| Ctrl+Shift+Q     | Close All Logs               |
| Ctrl+Shift+Left  | Focus Previous Logs          |
| Ctrl+Shift+Right | Focus Next Logs              |
