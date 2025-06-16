# Unity-PythonNet

## Development Environment

- Windows
- Unity6000.1.7f1 (.NET Framework is required at Api Compatibility Level)
- [Python.NET](https://www.nuget.org/packages/pythonnet) 3.0.5
- [Python Embeddable Package](https://www.python.org/downloads/windows/) 3.11.3

## What is this?

This is a sample project that integrates Unity and Python.NET. Pressing the *Plot* button will display an image of a graph drawn using matplotlib in Python.

<img src="https://raw.githubusercontent.com/shiena/Unity-PythonNet/main/Documents~/preview.png" title="preview">

## Project Structure

```
+ Assets/
  + Scenes/
    + SampleScene.unity          : Main scene using Unity UI
    + MainView.unity             : Main scene uning UI Toolkit
  + Scripts/
    + PythonLifeCycle.cs         : Python.NET initializer
    + PlotRandom.cs              : Call python script from C# using Python.NET for Unity UI
    + UIEventHandler.cs          : Call python script from C# using Python.NET for UI Toolkit
  + StreamingAssets/
    + python-3.11.3-embed-amd64/ : python runtime
    + myproject/
      + plot_random.py           : python script for graph plot
      + requirements.txt         : a list of items to be installed using pip install
```

## How to add python packages

```sh
cd Assets\StreamingAssets\python-3.11.3-embed-amd64\
.\Scripts\pip.exe install matplotlib
.\Scripts\pip.exe freeze > myproject\requirements.txt
```

## Known Issues

- `PythonEngine.PythonPath` is not applied when changing a different `PythonEngine.PythonPath` after calling `PythonEngine.Initialize()` / `PythonEngine.Shutdown()`.
  - Restart Unity if you want to set a different `PythonEngine.PythonPath`.