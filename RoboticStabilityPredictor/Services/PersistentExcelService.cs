using OfficeOpenXml;
using System;
using System.IO;

namespace RoboticStabilityPredictor.Services
{
    public class PersistentExcelService
    {
        private readonly string _excelFilePath;

        public PersistentExcelService()
        {
            // Set the license context for EPPlus (NonCommercial or Commercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Define the file path
            _excelFilePath = @"C:\RoboticStabilityData\StabilityData.xlsx";

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_excelFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize the Excel file if it doesn't exist
            if (!File.Exists(_excelFilePath))
            {
                InitializeExcelFile();
            }
        }

        private void InitializeExcelFile()
        {
            using (var package = new ExcelPackage())
            {
                // Define robot types
                var robotTypes = new[] { "ABB", "FANUC", "UR5", "KUKA" };

                foreach (var robotType in robotTypes)
                {
                    var worksheet = package.Workbook.Worksheets.Add(robotType);

                    // Add headers (removed Robot Type column, reordered: masses/lengths before SDD values, stability as numeric)
                    worksheet.Cells[1, 1].Value = "Number of Arms";
                    worksheet.Cells[1, 2].Value = "Force (N)";
                    worksheet.Cells[1, 3].Value = "Mass 1 (kg)";
                    worksheet.Cells[1, 4].Value = "Mass 2 (kg)";
                    worksheet.Cells[1, 5].Value = "Mass 3 (kg)";
                    worksheet.Cells[1, 6].Value = "Mass 4 (kg)";
                    worksheet.Cells[1, 7].Value = "Mass 5 (kg)";
                    worksheet.Cells[1, 8].Value = "Mass 6 (kg)";
                    worksheet.Cells[1, 9].Value = "Length 1 (m)";
                    worksheet.Cells[1, 10].Value = "Length 2 (m)";
                    worksheet.Cells[1, 11].Value = "Length 3 (m)";
                    worksheet.Cells[1, 12].Value = "Length 4 (m)";
                    worksheet.Cells[1, 13].Value = "Length 5 (m)";
                    worksheet.Cells[1, 14].Value = "Length 6 (m)";
                    worksheet.Cells[1, 15].Value = "Stiffness";
                    worksheet.Cells[1, 16].Value = "Damping";
                    worksheet.Cells[1, 17].Value = "Deflection";
                    worksheet.Cells[1, 18].Value = "Stability Score";
                    worksheet.Cells[1, 19].Value = "Low";
                    worksheet.Cells[1, 20].Value = "Medium";
                    worksheet.Cells[1, 21].Value = "High";

                    // Format header row
                    using (var range = worksheet.Cells[1, 1, 1, 21])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        range.AutoFitColumns();
                    }

                    // Seed 200 rows of sample data for each robot type
                    SeedSampleData(worksheet, robotType);
                }

                // Save the file
                package.SaveAs(new FileInfo(_excelFilePath));
            }
        }

        public void AppendCalculationData(
            string robotType,
            int numberOfArms,
            double force,
            double stiffness,
            double damping,
            double deflection,
            double[] masses,
            double[] lengths,
            string stabilityLevel)
        {
            using (var package = new ExcelPackage(new FileInfo(_excelFilePath)))
            {
                // Get or create the worksheet for the robot type
                var worksheet = package.Workbook.Worksheets[robotType];
                if (worksheet == null)
                {
                    throw new Exception($"Worksheet for robot type '{robotType}' not found.");
                }

                // Find the next empty row
                int nextRow = worksheet.Dimension?.End.Row + 1 ?? 2;

                // Calculate stability score (Stiffness / (Damping × Deflection))
                double stabilityScore = (damping > 0 && deflection > 0) ? stiffness / (damping * deflection) : 0;

                // Add data (reordered: NumberOfArms, Force, Masses, Lengths, SDD values, Stability Score)
                worksheet.Cells[nextRow, 1].Value = numberOfArms;
                worksheet.Cells[nextRow, 2].Value = force;

                // Add masses (N/A for unused arms)
                for (int i = 0; i < 6; i++)
                {
                    if (i < numberOfArms && i < masses.Length)
                    {
                        worksheet.Cells[nextRow, 3 + i].Value = masses[i];
                    }
                    else
                    {
                        worksheet.Cells[nextRow, 3 + i].Value = "N/A";
                    }
                }

                // Add lengths (N/A for unused arms)
                for (int i = 0; i < 6; i++)
                {
                    if (i < numberOfArms && i < lengths.Length)
                    {
                        worksheet.Cells[nextRow, 9 + i].Value = lengths[i];
                    }
                    else
                    {
                        worksheet.Cells[nextRow, 9 + i].Value = "N/A";
                    }
                }

                // Add SDD values
                worksheet.Cells[nextRow, 15].Value = stiffness;
                worksheet.Cells[nextRow, 16].Value = damping;
                worksheet.Cells[nextRow, 17].Value = deflection;
                worksheet.Cells[nextRow, 18].Value = stabilityScore;

                // Populate category columns (Low / Medium / High) using project thresholds:
                // Low: score < 1.5, Medium: 1.5 <= score < 3.5, High: score >= 3.5
                int low = 0, medium = 0, high = 0;
                if (stabilityScore < 1.5)
                    low = 1;
                else if (stabilityScore < 3.5)
                    medium = 1;
                else
                    high = 1;

                worksheet.Cells[nextRow, 19].Value = low;
                worksheet.Cells[nextRow, 20].Value = medium;
                worksheet.Cells[nextRow, 21].Value = high;

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                package.Save();
            }
        }

        private void SeedSampleData(ExcelWorksheet worksheet, string robotType)
        {
            Random random = new Random();
            
            for (int row = 2; row <= 501; row++) // 500 rows of data
            {
                int numberOfArms = random.Next(1, 7); // 1 to 6 arms
                double force = random.Next(500, 5001); // 500 to 5000 N

                // Add data
                worksheet.Cells[row, 1].Value = numberOfArms;
                worksheet.Cells[row, 2].Value = force;

                // Generate masses and lengths
                for (int i = 0; i < 6; i++)
                {
                    if (i < numberOfArms)
                    {
                        double mass = Math.Round(random.NextDouble() * 10 + 1, 2); // 1-11 kg
                        double length = Math.Round(random.NextDouble() * 1.5 + 0.2, 2); // 0.2-1.7 m
                        worksheet.Cells[row, 3 + i].Value = mass;
                        worksheet.Cells[row, 9 + i].Value = length;
                    }
                    else
                    {
                        worksheet.Cells[row, 3 + i].Value = "N/A";
                        worksheet.Cells[row, 9 + i].Value = "N/A";
                    }
                }

                // Generate realistic SDD values
                double stiffness = Math.Round(random.NextDouble() * 100000 + 10000, 2); // 10k-110k
                double damping = Math.Round(random.NextDouble() * 0.5 + 0.01, 4); // 0.01-0.51
                double deflection = Math.Round(random.NextDouble() * 0.001 + 0.0001, 6); // 0.0001-0.0011
                double stabilityScore = Math.Round(stiffness / (damping * deflection), 2);

                worksheet.Cells[row, 15].Value = stiffness;
                worksheet.Cells[row, 16].Value = damping;
                worksheet.Cells[row, 17].Value = deflection;
                worksheet.Cells[row, 18].Value = stabilityScore;

                // Determine category flags and write into new columns 19-21
                int low = 0, medium = 0, high = 0;
                if (stabilityScore < 1.5)
                    low = 1;
                else if (stabilityScore < 3.5)
                    medium = 1;
                else
                    high = 1;

                worksheet.Cells[row, 19].Value = low;
                worksheet.Cells[row, 20].Value = medium;
                worksheet.Cells[row, 21].Value = high;
            }
        }

        public string GetExcelFilePath()
        {
            return _excelFilePath;
        }
    }
}
