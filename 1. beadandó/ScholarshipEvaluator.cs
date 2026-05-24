using System;

namespace StudentPerformanceApp.Services
{
    public class ScholarshipEvaluator
    {
        public bool IsEligible(double average, int failedSubjects)
        {
            if (average < 1.0 || average > 5.0)
                throw new ArgumentException("Érvénytelen átlag.");

            if (failedSubjects < 0)
                throw new ArgumentException("Negatív bukás.");

            return average >= 4.0 && failedSubjects == 0;
        }
    }
}