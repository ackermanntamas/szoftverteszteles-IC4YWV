using System;
using System.Collections.Generic;
using System.Linq;
using StudentPerformanceApp.Models;

namespace StudentPerformanceApp.Services
{
    public class StudentPerformanceEvaluator
    {
        public double CalculateWeightedAverage(List<CourseRecord> records)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            if (!records.Any())
                throw new ArgumentException("Üres lista.");

            if (records.Any(r => r.Grade < 1 || r.Grade > 5))
                throw new ArgumentException("Érvénytelen jegy.");

            if (records.Any(r => r.Credit <= 0))
                throw new ArgumentException("Érvénytelen kredit.");

            return records.Sum(r => r.Grade * r.Credit) /
                   records.Sum(r => r.Credit);
        }

        public string DetermineClassification(double average)
        {
            if (average <= 2.0) return "Elégtelen";
            if (average <= 3.5) return "Közepes";
            if (average <= 4.5) return "Jó";
            return "Kiváló";
        }
    }
}