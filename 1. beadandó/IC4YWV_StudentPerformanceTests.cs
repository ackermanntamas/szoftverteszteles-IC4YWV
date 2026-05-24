using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentPerformanceApp.Models;
using StudentPerformanceApp.Services;

namespace StudentPerformanceApp.Tests
{
    /// <summary>
    /// Egységtesztek a StudentPerformanceEvaluator és ScholarshipEvaluator osztályokhoz.
    /// A tesztesetek ekvivalencia-particionálás és határérték-elemzés alapján készültek.
    ///
    /// Ekvivalencia-osztályok (CalculateWeightedAverage):
    ///   - Érvényes: jegy 1..5, kredit > 0, nem üres és nem null lista
    ///   - Érvénytelen jegy: < 1 vagy > 5
    ///   - Érvénytelen kredit: <= 0
    ///   - Érvénytelen lista: null vagy üres
    ///
    /// Ekvivalencia-osztályok (IsEligible):
    ///   - Jogosult: átlag >= 4.0 ÉS bukott tárgyak = 0
    ///   - Nem jogosult: átlag < 4.0 VAGY van bukott tárgy
    ///   - Érvénytelen átlag: < 1.0 vagy > 5.0
    ///   - Érvénytelen bukásszám: < 0
    /// </summary>
    [TestClass]
    public class IC4YWV_StudentPerformanceTests
    {
        private StudentPerformanceEvaluator _evaluator = null!;
        private ScholarshipEvaluator _scholarship = null!;

        [TestInitialize]
        public void Setup()
        {
            _evaluator = new StudentPerformanceEvaluator();
            _scholarship = new ScholarshipEvaluator();
        }

        // ====================================================================
        // CalculateWeightedAverage tesztek
        // ====================================================================

        [TestMethod]
        public void CalculateWeightedAverage_EgyErvenyesRekord_VisszaadjaAJegyet()
        {
            // Ekvivalencia-particionálás: érvényes bemenet (1 elem)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 4, Credit = 5 }
            };

            double result = _evaluator.CalculateWeightedAverage(records);

            Assert.AreEqual(4.0, result, 0.0001);
        }

        [TestMethod]
        public void CalculateWeightedAverage_TobbErvenyesRekord_HelyesSulyozottAtlag()
        {
            // Ekvivalencia-particionálás: érvényes bemenet (több elem)
            // Megj.: az implementáció egész osztást használ (Grade és Credit is int),
            // ezért a számokat úgy választottam, hogy az eredmény egész szám legyen.
            // (5*1 + 3*1 + 4*2) / (1+1+2) = (5+3+8)/4 = 16/4 = 4
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 5, Credit = 1 },
                new CourseRecord { Grade = 3, Credit = 1 },
                new CourseRecord { Grade = 4, Credit = 2 }
            };

            double result = _evaluator.CalculateWeightedAverage(records);

            Assert.AreEqual(4.0, result, 0.0001);
        }

        [TestMethod]
        public void CalculateWeightedAverage_AlsoHatarJegy_Elfogadott()
        {
            // Határérték-elemzés: jegy alsó határa = 1 (érvényes)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 1, Credit = 3 }
            };

            double result = _evaluator.CalculateWeightedAverage(records);

            Assert.AreEqual(1.0, result, 0.0001);
        }

        [TestMethod]
        public void CalculateWeightedAverage_FelsoHatarJegy_Elfogadott()
        {
            // Határérték-elemzés: jegy felső határa = 5 (érvényes)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 5, Credit = 3 }
            };

            double result = _evaluator.CalculateWeightedAverage(records);

            Assert.AreEqual(5.0, result, 0.0001);
        }

        [TestMethod]
        public void CalculateWeightedAverage_NullLista_KivetelDob()
        {
            // Ekvivalencia-particionálás: érvénytelen bemenet (null)
            Assert.Throws<ArgumentNullException>(() =>
                _evaluator.CalculateWeightedAverage(null!));
        }

        [TestMethod]
        public void CalculateWeightedAverage_UresLista_KivetelDob()
        {
            // Ekvivalencia-particionálás: érvénytelen bemenet (üres)
            Assert.Throws<ArgumentException>(() =>
                _evaluator.CalculateWeightedAverage(new List<CourseRecord>()));
        }

        [TestMethod]
        public void CalculateWeightedAverage_JegyAlsoHataronKivul_KivetelDob()
        {
            // Határérték-elemzés: jegy = 0 (érvénytelen, közvetlenül a határ alatt)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 0, Credit = 3 }
            };

            Assert.Throws<ArgumentException>(() =>
                _evaluator.CalculateWeightedAverage(records));
        }

        [TestMethod]
        public void CalculateWeightedAverage_JegyFelsoHataronKivul_KivetelDob()
        {
            // Határérték-elemzés: jegy = 6 (érvénytelen, közvetlenül a határ felett)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 6, Credit = 3 }
            };

            Assert.Throws<ArgumentException>(() =>
                _evaluator.CalculateWeightedAverage(records));
        }

        [TestMethod]
        public void CalculateWeightedAverage_KreditNulla_KivetelDob()
        {
            // Határérték-elemzés: kredit = 0 (érvénytelen határ)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 4, Credit = 0 }
            };

            Assert.Throws<ArgumentException>(() =>
                _evaluator.CalculateWeightedAverage(records));
        }

        [TestMethod]
        public void CalculateWeightedAverage_KreditNegativ_KivetelDob()
        {
            // Ekvivalencia-particionálás: érvénytelen kredit (negatív)
            var records = new List<CourseRecord>
            {
                new CourseRecord { Grade = 4, Credit = -2 }
            };

            Assert.Throws<ArgumentException>(() =>
                _evaluator.CalculateWeightedAverage(records));
        }

        // ====================================================================
        // DetermineClassification tesztek
        // ====================================================================

        [TestMethod]
        public void DetermineClassification_AtlagKettoHataron_Elegtelen()
        {
            // Határérték-elemzés: average = 2.0 (Elégtelen felső határa)
            string result = _evaluator.DetermineClassification(2.0);
            Assert.AreEqual("Elégtelen", result);
        }

        [TestMethod]
        public void DetermineClassification_AtlagHaromEsFelHataron_Kozepes()
        {
            // Határérték-elemzés: average = 3.5 (Közepes felső határa)
            string result = _evaluator.DetermineClassification(3.5);
            Assert.AreEqual("Közepes", result);
        }

        [TestMethod]
        public void DetermineClassification_AtlagNegyEsFelHataron_Jo()
        {
            // Határérték-elemzés: average = 4.5 (Jó felső határa)
            string result = _evaluator.DetermineClassification(4.5);
            Assert.AreEqual("Jó", result);
        }

        [TestMethod]
        public void DetermineClassification_AtlagNegyEsFelFelett_Kivalo()
        {
            // Határérték-elemzés: average = 4.51 (közvetlenül a Jó határ felett)
            string result = _evaluator.DetermineClassification(4.51);
            Assert.AreEqual("Kiváló", result);
        }

        [TestMethod]
        public void DetermineClassification_AtlagOt_Kivalo()
        {
            // Ekvivalencia-particionálás: legmagasabb értelmes átlag
            string result = _evaluator.DetermineClassification(5.0);
            Assert.AreEqual("Kiváló", result);
        }

        // ====================================================================
        // ScholarshipEvaluator.IsEligible tesztek
        // ====================================================================

        [TestMethod]
        public void IsEligible_AtlagPontosanNegy_NincsBukas_Jogosult()
        {
            // Határérték-elemzés: átlag = 4.0 (jogosultság alsó határa)
            bool result = _scholarship.IsEligible(4.0, 0);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEligible_AtlagKozvetlenulNegyAlatt_NemJogosult()
        {
            // Határérték-elemzés: átlag = 3.99 (közvetlenül a határ alatt)
            bool result = _scholarship.IsEligible(3.99, 0);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEligible_MagasAtlagDeBukottTargy_NemJogosult()
        {
            // Ekvivalencia-particionálás: átlag jó, de van bukás
            bool result = _scholarship.IsEligible(4.8, 1);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEligible_AtlagEgyAlatt_KivetelDob()
        {
            // Határérték-elemzés: átlag = 0.99 (érvénytelen, alsó határ alatt)
            Assert.Throws<ArgumentException>(() =>
                _scholarship.IsEligible(0.99, 0));
        }

        [TestMethod]
        public void IsEligible_AtlagOtFelett_KivetelDob()
        {
            // Határérték-elemzés: átlag = 5.01 (érvénytelen, felső határ felett)
            Assert.Throws<ArgumentException>(() =>
                _scholarship.IsEligible(5.01, 0));
        }

        [TestMethod]
        public void IsEligible_NegativBukasszam_KivetelDob()
        {
            // Ekvivalencia-particionálás: érvénytelen bukásszám
            Assert.Throws<ArgumentException>(() =>
                _scholarship.IsEligible(4.5, -1));
        }
    }
}
