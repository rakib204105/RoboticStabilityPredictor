# Robotic Stability Predictor 🤖

A robust ASP.NET Core web application designed to predict the stability of multi-arm robotic systems. This tool helps engineers and researchers determine if a robotic configuration will be stable based on mechanical properties.

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Status](https://img.shields.io/badge/Status-Active-success)

## 🚀 Features

- **Stability Prediction**: Calculates 'Low', 'Medium', or 'High' stability based on stiffness, damping, and deflection.
- **Multi-Arm Configurator**: Support for variable number of robotic arms with custom parameters.
- **Material Library**: Built-in properties for Steel, Aluminum, Titanium, Carbon Fiber, Brass, and Copper.
- **Research Data Collection**: Automatically saves calculation results to a master Excel file for analysis.
- **User Authentication**: Secure user registration and login system.
- **Responsive Design**: Modern UI that works on desktop and mobile.

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C#
- **Database**: SQL Server / SQLite (coming soon)
- **Frontend**: Razor Pages, HTML5, CSS3, JavaScript
- **Libraries**:
  - `EPPlus` for Excel data handling
  - ASP.NET Identity for authentication

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🏃‍♂️ Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/RoboticStabilityPredictor.git
   cd RoboticStabilityPredictor
   ```

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run --project RoboticStabilityPredictor
   ```

4. **Access the app**
   Open your browser and navigate to `http://localhost:5222`

## 📊 Data Collection

The application features a "Save to Master File" button that records:
- Robot configurations (Type, Arms, Force)
- Calculated mechanical properties (Stiffness, Damping, Deflection)
- Individual arm parameters (Mass, Length)
- Stability verdict

Data is saved locally for research and analysis purposes.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
