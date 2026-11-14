# Native AOT smoke test

This headless executable uses the real SukiUI theme. It checks all compactness dictionaries,
switches all four global levels, applies a local scope, and resolves and evaluates every easing
preset. Run the published native executable: a normal managed build cannot detect these regressions.

On Windows with .NET 10 SDK and the Visual Studio C++ build tools installed:

```powershell
dotnet publish SukiUI.AotSmokeTests -c Release -r win-x64 -o artifacts/aot-smoke
./artifacts/aot-smoke/SukiUI.AotSmokeTests.exe
```

The Native AOT smoke test workflow also runs this on Windows. The untrimmed Motion tests independently
compare the easing catalog with the public preset declarations, including their order and identity.
