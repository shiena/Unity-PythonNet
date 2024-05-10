# Unity-PythonNet

## Development Environment

- Unity2022.3.22f1
- [Python.NET](https://www.nuget.org/packages/pythonnet) 3.0.3
- [Python Embeddable Package](https://www.python.org/downloads/windows/) 3.11.3

## What is this?

This is a sample project that integrates Unity and Python.NET. Pressing the *Plot* button will display an image of a graph drawn using matplotlib in Python.

<img src="https://raw.githubusercontent.com/shiena/Unity-PythonNet/main/Documents~/preview.png" title="preview">

## Project Structure

```
+ Assets/
  + Scenes/
    + MainView.unity             : Main scene uning UI Toolkit
    + SampleScene.unity          : Main scene using Unity UI
  + Scripts/
    + PythonLifeCycle.cs         : Python.NET initializer
    + PlotRandom.cs              : Call python script from C# using Python.NET
  + StreamingAssets/
    + python-3.11.3-embed-amd64/ : python runtime
    + myproject/
      + plot_random.py           : python script for graph plot
      + requirements.txt         : a list of items to be installed using pip install
```
