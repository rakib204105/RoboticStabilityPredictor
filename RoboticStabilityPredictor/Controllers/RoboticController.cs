using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RoboticStabilityPredictor.Services;

namespace RoboticStabilityPredictor.Controllers
{
    public class RoboticController : Controller
    {
        private readonly StabilityCalculationService _calculationService;
        private readonly PersistentExcelService _excelService;

        public RoboticController(StabilityCalculationService calculationService, PersistentExcelService excelService)
        {
            _calculationService = calculationService;
            _excelService = excelService;
        }

        [HttpGet]
        public IActionResult InputRobotType()
        {
            return PartialView("_InputRobotType");
        }

        [HttpGet]
        public IActionResult InputNumberOfArms(string robotType)
        {
            ViewBag.RobotType = robotType;
            return View("_InputNumberOfArms");
        }

        [HttpPost]
        public IActionResult CommonInputs(string robotType, int numberOfArms)
        {
            ViewBag.RobotType = robotType;
            ViewBag.NumberOfArms = numberOfArms;
            return PartialView("_CommonInputs");
        }

        [HttpPost]
        public IActionResult MaterialSelection(string robotType, int numberOfArms)
        {
            ViewBag.RobotType = robotType;
            ViewBag.NumberOfArms = numberOfArms;
            return PartialView("_MaterialSelection");
        }

        [HttpPost]
        public IActionResult ForceInput(string robotType, int numberOfArms, string material)
        {
            ViewBag.RobotType = robotType;
            ViewBag.NumberOfArms = numberOfArms;
            ViewBag.Material = material;
            return PartialView("_ForceInput");
        }

        [HttpPost]
        public IActionResult ArmParameters(string robotType, int numberOfArms, string material, decimal force)
        {
            ViewBag.RobotType = robotType;
            ViewBag.NumberOfArms = numberOfArms;
            ViewBag.Material = material;
            ViewBag.Force = force;
            return PartialView("_ArmParameters");
        }

        [HttpPost]
        public IActionResult CalculateStability(string robotType, int numberOfArms, string material, decimal force, 
            decimal length1, decimal length2, decimal length3, decimal length4, decimal length5, decimal length6,
            decimal outerDiameter1, decimal outerDiameter2, decimal outerDiameter3, decimal outerDiameter4, decimal outerDiameter5, decimal outerDiameter6,
            decimal thickness1, decimal thickness2, decimal thickness3, decimal thickness4, decimal thickness5, decimal thickness6)
        {
            // Get the actual values from the form using the hyphenated field names
            var form = Request.Form;
            
            // Debug: Log all form keys and values
            System.Diagnostics.Debug.WriteLine("=== Form Data Debug ===");
            foreach (var key in form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"Form Key: '{key}' = '{form[key]}'");
            }
            System.Diagnostics.Debug.WriteLine("=== End Form Data ===");
            
            // Extract values from form with hyphenated names
            var lengths = new decimal[6];
            var outerDiameters = new decimal[6];
            var thicknesses = new decimal[6];

            for (int i = 1; i <= 6; i++)
            {
                // Try to get values from hyphenated field names
                if (form.ContainsKey($"length-{i}"))
                {
                    decimal.TryParse(form[$"length-{i}"], out lengths[i - 1]);
                }
                else if (form.ContainsKey($"length{i}"))
                {
                    decimal.TryParse(form[$"length{i}"], out lengths[i - 1]);
                }

                if (form.ContainsKey($"outerDiameter-{i}"))
                {
                    decimal.TryParse(form[$"outerDiameter-{i}"], out outerDiameters[i - 1]);
                }
                else if (form.ContainsKey($"outerDiameter{i}"))
                {
                    decimal.TryParse(form[$"outerDiameter{i}"], out outerDiameters[i - 1]);
                }

                if (form.ContainsKey($"thickness-{i}"))
                {
                    decimal.TryParse(form[$"thickness-{i}"], out thicknesses[i - 1]);
                }
                else if (form.ContainsKey($"thickness{i}"))
                {
                    decimal.TryParse(form[$"thickness{i}"], out thicknesses[i - 1]);
                }
            }

            // Debug: Log extracted values
            System.Diagnostics.Debug.WriteLine("=== Extracted Values ===");
            for (int i = 0; i < numberOfArms; i++)
            {
                System.Diagnostics.Debug.WriteLine($"Arm {i + 1}: Length={lengths[i]}, OuterDiam={outerDiameters[i]}, Thickness={thicknesses[i]}");
            }
            System.Diagnostics.Debug.WriteLine("=== End Extracted Values ===");

            // Material properties (Young's modulus in MPa, Density in kg/m³)
            var materialProperties = new Dictionary<string, (decimal youngsModulus, decimal density)>
            {
                {"steel", (200000, 7850)},
                {"aluminum", (70000, 2700)},
                {"titanium", (116000, 4500)},
                {"carbonFiber", (230000, 1600)},
                {"brass", (105000, 8500)},
                {"copper", (110000, 8960)}
            };

            var (youngsModulus, density) = materialProperties[material];

            // Use the same Young's modulus for both min and max (as in your original code)
            decimal minYoungModulus = youngsModulus;
            decimal maxYoungModulus = youngsModulus;

            // Calculate SDD values using the EXACT formulas from your original code
            var sddValues = CalculateSDDValuesExact(robotType, numberOfArms, force, minYoungModulus, maxYoungModulus, density,
                lengths, outerDiameters, thicknesses);

            // Calculate stability level using the formula
            string stabilityLevel = _calculationService.CalculateStability(
                sddValues.MediumStiffness, 
                sddValues.MediumDeflection, 
                sddValues.MediumDamping
            );

            ViewBag.StabilityLevel = stabilityLevel;
            ViewBag.SDDValues = sddValues;
            ViewBag.InputData = new
            {
                RobotType = robotType,
                NumberOfArms = numberOfArms,
                Material = material,
                Force = force
            };
            
            // Pass individual arm parameters for Excel export
            var armParameters = new List<object>();
            const decimal PI = 3.14159265358979323846M;
            
            System.Diagnostics.Debug.WriteLine("=== Building Arm Parameters for ViewBag ===");
            System.Diagnostics.Debug.WriteLine($"Number of Arms: {numberOfArms}");
            System.Diagnostics.Debug.WriteLine($"Material: {material}, Density: {density}");
            
            for (int i = 0; i < numberOfArms; i++)
            {
                try 
                {
                    decimal length = (i < lengths.Length) ? lengths[i] : 0;
                    decimal outerDiameter = (i < outerDiameters.Length) ? outerDiameters[i] : 0;
                    decimal thickness = (i < thicknesses.Length) ? thicknesses[i] : 0;
                    
                    System.Diagnostics.Debug.WriteLine($"Arm {i + 1} raw values: L={length}, OD={outerDiameter}, T={thickness}");
                    
                    decimal thicknessM = thickness / 1000M;
                    decimal innerDiameter = outerDiameter > 0 ? outerDiameter - (2 * thicknessM) : 0;
                    decimal volume = (length > 0 && outerDiameter > 0 && innerDiameter > 0) ? 
                        PI * length * (outerDiameter * outerDiameter - innerDiameter * innerDiameter) / 4M : 0;
                    decimal mass = volume > 0 ? volume * density : 0;
                    
                    System.Diagnostics.Debug.WriteLine($"Arm {i + 1} calculated: ID={innerDiameter}, Vol={volume}, Mass={mass}");
                    
                    armParameters.Add(new
                    {
                        ArmNumber = i + 1,
                        Length = length,
                        OuterDiameter = outerDiameter,
                        InnerDiameter = Math.Max(0, innerDiameter), // Ensure non-negative
                        Thickness = thickness,
                        Mass = Math.Max(0, mass), // Ensure non-negative
                        Volume = Math.Max(0, volume) // Ensure non-negative
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"Arm {i + 1} added to armParameters list");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating parameters for arm {i + 1}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Add placeholder data for this arm
                    armParameters.Add(new
                    {
                        ArmNumber = i + 1,
                        Length = 0m,
                        OuterDiameter = 0m,
                        InnerDiameter = 0m,
                        Thickness = 0m,
                        Mass = 0m,
                        Volume = 0m
                    });
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"=== Total Arm Parameters Added: {armParameters.Count} ===");
            foreach (var arm in armParameters)
            {
                dynamic armData = arm;
                System.Diagnostics.Debug.WriteLine($"Final Arm {armData.ArmNumber}: L={armData.Length}, OD={armData.OuterDiameter}, T={armData.Thickness}, M={armData.Mass}");
            }
            
            ViewBag.ArmParameters = armParameters;
            System.Diagnostics.Debug.WriteLine("ViewBag.ArmParameters has been set!");

            // Store data in TempData for persistent Excel save (convert to strings for serialization)
            TempData["RobotType"] = robotType;
            TempData["NumberOfArms"] = numberOfArms;
            TempData["Force"] = force.ToString();
            TempData["Stiffness"] = sddValues.MediumStiffness.ToString();
            TempData["Damping"] = sddValues.MediumDamping.ToString();
            TempData["Deflection"] = sddValues.MediumDeflection.ToString();
            TempData["StabilityLevel"] = stabilityLevel;
            
            // Store masses and lengths arrays
            var massArray = new double[numberOfArms];
            var lengthArray = new double[numberOfArms];
            for (int i = 0; i < numberOfArms; i++)
            {
                dynamic armData = armParameters[i];
                massArray[i] = (double)armData.Mass;
                lengthArray[i] = (double)armData.Length;
            }
            TempData["Masses"] = JsonSerializer.Serialize(massArray);
            TempData["Lengths"] = JsonSerializer.Serialize(lengthArray);
            TempData.Keep(); // Keep data for next request

            return View("FinalResult");
        }

        [HttpPost]
        public IActionResult SaveToPersistentExcel()
        {
            try
            {
                // Retrieve data from TempData (set it in CalculateStability if needed)
                var robotType = TempData["RobotType"]?.ToString();
                var numberOfArmsStr = TempData["NumberOfArms"]?.ToString();
                var forceStr = TempData["Force"]?.ToString();
                var stiffnessStr = TempData["Stiffness"]?.ToString();
                var dampingStr = TempData["Damping"]?.ToString();
                var deflectionStr = TempData["Deflection"]?.ToString();
                var massesJson = TempData["Masses"]?.ToString();
                var lengthsJson = TempData["Lengths"]?.ToString();
                var stabilityLevel = TempData["StabilityLevel"]?.ToString();

                // Debug logging
                System.Diagnostics.Debug.WriteLine("=== SaveToPersistentExcel Debug ===");
                System.Diagnostics.Debug.WriteLine($"RobotType: {robotType}");
                System.Diagnostics.Debug.WriteLine($"NumberOfArms: {numberOfArmsStr}");
                System.Diagnostics.Debug.WriteLine($"Force: {forceStr}");
                System.Diagnostics.Debug.WriteLine($"Stiffness: {stiffnessStr}");
                System.Diagnostics.Debug.WriteLine($"Damping: {dampingStr}");
                System.Diagnostics.Debug.WriteLine($"Deflection: {deflectionStr}");
                System.Diagnostics.Debug.WriteLine($"Masses: {massesJson}");
                System.Diagnostics.Debug.WriteLine($"Lengths: {lengthsJson}");
                System.Diagnostics.Debug.WriteLine($"StabilityLevel: {stabilityLevel}");

                // Parse data with detailed error messages
                if (string.IsNullOrEmpty(numberOfArmsStr))
                    return Json(new { success = false, message = "Number of arms is missing" });
                if (!int.TryParse(numberOfArmsStr, out int numberOfArms))
                    return Json(new { success = false, message = $"Invalid number of arms: {numberOfArmsStr}" });

                if (string.IsNullOrEmpty(forceStr))
                    return Json(new { success = false, message = "Force is missing" });
                if (!double.TryParse(forceStr, out double force))
                    return Json(new { success = false, message = $"Invalid force value: {forceStr}" });

                if (string.IsNullOrEmpty(stiffnessStr))
                    return Json(new { success = false, message = "Stiffness is missing" });
                if (!double.TryParse(stiffnessStr, out double stiffness))
                    return Json(new { success = false, message = $"Invalid stiffness value: {stiffnessStr}" });

                if (string.IsNullOrEmpty(dampingStr))
                    return Json(new { success = false, message = "Damping is missing" });
                if (!double.TryParse(dampingStr, out double damping))
                    return Json(new { success = false, message = $"Invalid damping value: {dampingStr}" });

                if (string.IsNullOrEmpty(deflectionStr))
                    return Json(new { success = false, message = "Deflection is missing" });
                if (!double.TryParse(deflectionStr, out double deflection))
                    return Json(new { success = false, message = $"Invalid deflection value: {deflectionStr}" });

                // Parse masses and lengths arrays
                var masses = JsonSerializer.Deserialize<double[]>(massesJson ?? "[]") ?? Array.Empty<double>();
                var lengths = JsonSerializer.Deserialize<double[]>(lengthsJson ?? "[]") ?? Array.Empty<double>();

                System.Diagnostics.Debug.WriteLine($"Parsed successfully - calling AppendCalculationData");

                // Save to Excel
                _excelService.AppendCalculationData(
                    robotType ?? "Unknown",
                    numberOfArms,
                    force,
                    stiffness,
                    damping,
                    deflection,
                    masses,
                    lengths,
                    stabilityLevel ?? "Unknown"
                );

                return Json(new { success = true, message = "Data saved successfully to master Excel file!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving to Excel: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // EXACT implementation matching your original code logic
        private SDDValues CalculateSDDValuesExact(string robotType, int numberOfArms, decimal force, 
            decimal minYoungModulus, decimal maxYoungModulus, decimal density,
            decimal[] lengths, decimal[] outerDiameters, decimal[] thicknesses)
        {
            const decimal PI = 3.14159265358979323846M;

            // Step 1: Calculate all deflections first (EXACTLY as in your original code)
            var deflectionsMin = new List<decimal>();
            var deflectionsMax = new List<decimal>();
            var masses = new List<decimal>();

            for (int i = 0; i < numberOfArms; i++)
            {
                if (lengths[i] <= 0 || outerDiameters[i] <= 0 || thicknesses[i] <= 0) continue;

                decimal thicknessM = thicknesses[i] / 1000M;
                decimal innerDiameter = outerDiameters[i] - (2 * thicknessM);
                if (innerDiameter <= 0) continue;

                try
                {
                    // Calculate area moment of inertia (EXACTLY as in your original code)
                    decimal areaMomentOfInertia = (PI / 64.0M) * 
                        ((outerDiameters[i] * outerDiameters[i] * outerDiameters[i] * outerDiameters[i]) - 
                         (innerDiameter * innerDiameter * innerDiameter * innerDiameter));

                    if (areaMomentOfInertia <= 0) continue;

                    // Calculate mass (EXACTLY as in your original code)
                    decimal volume = PI * lengths[i] * (outerDiameters[i] * outerDiameters[i] - innerDiameter * innerDiameter) / 4M;
                    decimal mass = volume * density;
                    if (mass <= 0) continue;

                    masses.Add(mass);

                    // Calculate deflections for both min and max Young's modulus (EXACTLY as in your original code)
                    decimal deflectionMin = (force * lengths[i] * lengths[i] * lengths[i]) / (3 * minYoungModulus * areaMomentOfInertia);
                    decimal deflectionMax = (force * lengths[i] * lengths[i] * lengths[i]) / (3 * maxYoungModulus * areaMomentOfInertia);

                    if (deflectionMin > 0) deflectionsMin.Add(deflectionMin);
                    if (deflectionMax > 0) deflectionsMax.Add(deflectionMax);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating deflections for arm {i + 1}: {ex.Message}");
                    continue;
                }
            }

            if (deflectionsMin.Count == 0 || deflectionsMax.Count == 0)
            {
                return new SDDValues { MinDeflection = 0, MediumDeflection = 0, MaxDeflection = 0, MinStiffness = 0, MediumStiffness = 0, MaxStiffness = 0, MinDamping = 0, MediumDamping = 0, MaxDamping = 0 };
            }

            // Step 2: Find max deflections (EXACTLY as in your original code)
            decimal maxDeflectionMin = deflectionsMin.Max();
            decimal maxDeflectionMax = deflectionsMax.Max();

            // Step 3: Calculate spring constants (EXACTLY as in your original code)
            decimal minSpringConstant = force / maxDeflectionMin;
            decimal maxSpringConstant = force / maxDeflectionMax;

            // Step 4: Calculate stiffness for each arm (EXACTLY as in your original code)
            var allStiffnesses = new List<decimal>();
            var allDampings = new List<decimal>();

            for (int i = 0; i < numberOfArms; i++)
            {
                if (lengths[i] <= 0 || outerDiameters[i] <= 0 || thicknesses[i] <= 0) continue;

                decimal thicknessM = thicknesses[i] / 1000M;
                decimal innerDiameter = outerDiameters[i] - (2 * thicknessM);
                if (innerDiameter <= 0) continue;

                try
                {
                    decimal volume = PI * lengths[i] * (outerDiameters[i] * outerDiameters[i] - innerDiameter * innerDiameter) / 4M;
                    decimal mass = volume * density;
                    if (mass <= 0) continue;

                    // Calculate stiffness using min spring constant (EXACTLY as in your original code)
                    // Stiffness = Spring Constant (k) - this is the direct relationship
                    decimal stiffnessMin = minSpringConstant;
                    if (stiffnessMin > 0) allStiffnesses.Add(stiffnessMin);

                    // Calculate stiffness using max spring constant (EXACTLY as in your original code)
                    decimal stiffnessMax = maxSpringConstant;
                    if (stiffnessMax > 0) allStiffnesses.Add(stiffnessMax);

                    // Calculate damping (EXACTLY as in your original code)
                    decimal dampingCoefficient = (decimal)Math.Sqrt((double)(8.967M / ((4 * PI * PI * mass * mass) + 8.967M)));
                    
                    // Calculate damping for both min and max stiffness
                    decimal criticalDampingCoefficientMin = (decimal)Math.Sqrt((double)(4 * mass * stiffnessMin));
                    decimal criticalDampingCoefficientMax = (decimal)Math.Sqrt((double)(4 * mass * stiffnessMax));
                    
                    if (criticalDampingCoefficientMin > 0)
                    {
                        decimal dampingMin = dampingCoefficient / criticalDampingCoefficientMin;
                        if (dampingMin > 0) allDampings.Add(dampingMin);
                    }
                    
                    if (criticalDampingCoefficientMax > 0)
                    {
                        decimal dampingMax = dampingCoefficient / criticalDampingCoefficientMax;
                        if (dampingMax > 0) allDampings.Add(dampingMax);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating stiffness/damping for arm {i + 1}: {ex.Message}");
                    continue;
                }
            }

            // Combine all deflections
            var allDeflections = new List<decimal>();
            allDeflections.AddRange(deflectionsMin);
            allDeflections.AddRange(deflectionsMax);

            // Sort all values (EXACTLY as in your original code)
            allDeflections.Sort();
            allStiffnesses.Sort();
            allDampings.Sort();

            System.Diagnostics.Debug.WriteLine($"Total calculations: Deflections={allDeflections.Count}, Stiffnesses={allStiffnesses.Count}, Dampings={allDampings.Count}");

            // Calculate min, medium, max values (EXACTLY as in your original code)
            return new SDDValues
            {
                MinDeflection = allDeflections.Count > 0 ? allDeflections.First() : 0,
                MediumDeflection = allDeflections.Count > 1 ? (allDeflections[allDeflections.Count / 2 - 1] + allDeflections[allDeflections.Count / 2]) / 2 : allDeflections.FirstOrDefault(),
                MaxDeflection = allDeflections.Count > 0 ? allDeflections.Last() : 0,
                MinStiffness = allStiffnesses.Count > 0 ? allStiffnesses.First() : 0,
                MediumStiffness = allStiffnesses.Count > 1 ? (allStiffnesses[allStiffnesses.Count / 2 - 1] + allStiffnesses[allStiffnesses.Count / 2]) / 2 : allStiffnesses.FirstOrDefault(),
                MaxStiffness = allStiffnesses.Count > 0 ? allStiffnesses.Last() : 0,
                MinDamping = allDampings.Count > 0 ? allDampings.First() : 0,
                MediumDamping = allDampings.Count > 1 ? (allDampings[allDampings.Count / 2 - 1] + allDampings[allDampings.Count / 2]) / 2 : allDampings.FirstOrDefault(),
                MaxDamping = allDampings.Count > 0 ? allDampings.Last() : 0
            };
        }

        [HttpPost]
        public IActionResult FinalResult(string robotType, int numberOfArms, decimal damping, decimal deflection, decimal stiffness)
        {
            string stabilityLevel = "Low";

            try
            {
                if (damping != 0 && deflection != 0)
                {
                    var score = (stiffness / (damping * deflection));
                    if (score < 1.5M)
                        stabilityLevel = "Low";
                    else if (score < 3.5M)
                        stabilityLevel = "Medium";
                    else
                        stabilityLevel = "High";
                }
            }
            catch (DivideByZeroException)
            {
                stabilityLevel = "Error: Invalid calculations";
            }

            ViewBag.StabilityLevel = stabilityLevel;
            
            // Set up basic data for Excel export (this path doesn't have detailed arm parameters)
            ViewBag.InputData = new
            {
                RobotType = robotType ?? "Unknown",
                NumberOfArms = numberOfArms,
                Material = "Not specified",
                Force = "Not specified"
            };
            
            // Create basic SDD values from the provided parameters
            ViewBag.SDDValues = new SDDValues
            {
                MinStiffness = stiffness,
                MediumStiffness = stiffness,
                MaxStiffness = stiffness,
                MinDeflection = deflection,
                MediumDeflection = deflection,
                MaxDeflection = deflection,
                MinDamping = damping,
                MediumDamping = damping,
                MaxDamping = damping
            };
            
            // No detailed arm parameters available for this path - will show as N/A in Excel
            ViewBag.ArmParameters = new List<object>();
            
            return View();
        }
    }

    public class SDDValues
    {
        public decimal MinDeflection { get; set; }
        public decimal MediumDeflection { get; set; }
        public decimal MaxDeflection { get; set; }
        public decimal MinStiffness { get; set; }
        public decimal MediumStiffness { get; set; }
        public decimal MaxStiffness { get; set; }
        public decimal MinDamping { get; set; }
        public decimal MediumDamping { get; set; }
        public decimal MaxDamping { get; set; }
    }
}
