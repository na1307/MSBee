# MSBee

**MSBee** is a Visual Studio extension that allows you to target **.NET Framework 1.0 and 1.1** in Visual Studio **2017 and earlier**—versions that no longer support these frameworks natively. The [original MSBee](https://github.com/na1307/MSBee/tree/original) was written for Visual Studio 2005, but I've reimagined it for modern use.

> ⚠️ Primarily intended for legacy system maintenance or compatibility testing or just fun.

The original MSBee 1.0 can be downloaded from [this link](https://github.com/na1307/MSBee/raw/refs/heads/original/MSBee%201.0%20Release.zip).

---

## 🛠️ Installation

Follow these steps to set up MSBee:

1. **Install .NET Framework 2.0/3.0/3.5**
   - This is required to build using MSBee. It is not pre-installed in Windows 8 or later, so you need to install it separately.
2. **Install .NET Framework 1.0 or 1.1**
   - These are required to build and run legacy applications.
3. **Install the corresponding SDK**
   - Install either the .NET Framework 1.0 SDK or 1.1 SDK.
4. **Download the MSBee extension**
   - Get the VSIX (for VS2017) or MSI (for VS2012 - VS2015) from the [latest release](https://github.com/na1307/MSBee/releases/latest).
5. **Launch Visual Studio**
   - Create a new project targeting .NET Framework 1.0 or 1.1.

---

## ❓ Frequently Asked Questions

### Why is MSBee limited to Visual Studio 2017 and earlier?

Newer versions (2019 and later) were tested but failed to correctly recognize the source code files in the project, making them unreliable for use with MSBee. Visual Studio 2017 is currently the latest stable version supported.

### What version of C# can I use?

You're limited to **C# 1.0 or 1.2**, depending on the .NET Framework version. This means many modern language features are unavailable, including:

- File-scoped namespaces
- Top-level statements
- Nullable value/reference types
- Tuples
- Expression-bodied members
- `async`/`await`
- Lambda expressions
- Generics

### Can I use Roslyn or ReSharper?

Yes, but with caution. These tools may suggest or enforce language features not supported by C# 1.x. Disable or adjust their rules accordingly.

---

## 🧩 How to Build the Extension

1. Install **Visual Studio 2022** and **[HeatWave for VS2022](https://marketplace.visualstudio.com/items?itemName=FireGiant.FireGiantHeatWaveDev17) extension** (required for building the extension).
2. Clone this repository:
   ```bash
   git clone https://github.com/na1307/MSBee.git
   ```
3. Open the `.sln` file in Visual Studio.
4. Go to **Build > Build Solution** or press `Ctrl + Shift + B`.

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
