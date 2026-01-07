namespace RoboticStabilityPredictor.Services
{
    /// <summary>
    /// Service for calculating stability level based on stiffness, deflection, and damping values.
    /// Uses a formula-based approach: score = stiffness / (damping * deflection)
    /// </summary>
    public class StabilityCalculationService
    {
        /// <summary>
        /// Calculates the stability level (Low, Medium, or High) based on mechanical properties.
        /// </summary>
        /// <param name="stiffness">Stiffness value of the robotic arm</param>
        /// <param name="deflection">Deflection value of the robotic arm</param>
        /// <param name="damping">Damping coefficient of the robotic arm</param>
        /// <returns>Stability level: "Low", "Medium", or "High"</returns>
        public string CalculateStability(decimal stiffness, decimal deflection, decimal damping)
        {
            try
            {
                // Avoid division by zero
                if (damping == 0 || deflection == 0)
                    return "Low";

                // Calculate stability score using the formula
                decimal score = stiffness / (damping * deflection);
                
                // Determine stability level based on thresholds
                if (score < 1.5M)
                    return "Low";
                else if (score < 3.5M)
                    return "Medium";
                else
                    return "High";
            }
            catch
            {
                // Default to Low stability in case of any calculation errors
                return "Low";
            }
        }
    }
} 