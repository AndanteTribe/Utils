using System.Collections.Generic;
using AndanteTribe.Utils.GameServices;
using NUnit.Framework;

namespace AndanteTribe.Utils.Tests
{
    public class GameServicesTest
    {
        // Obscured<T> tests
        [Test]
        public void ObscuredIntImplicitConversions()
        {
            // Test implicit conversion from int to Obscured<int>
            Obscured<int> obscured = 42;

            // Test implicit conversion from Obscured<int> to int
            int value = obscured;

            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void ObscuredFloatConversions()
        {
            float originalValue = 3.14f;
            Obscured<float> obscured = originalValue;
            float retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredDoubleConversions()
        {
            double originalValue = 2.718281828;
            Obscured<double> obscured = originalValue;
            double retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredBoolConversions()
        {
            // Test true
            Obscured<bool> obscuredTrue = true;
            bool retrievedTrue = obscuredTrue;
            Assert.That(retrievedTrue, Is.True);

            // Test false
            Obscured<bool> obscuredFalse = false;
            bool retrievedFalse = obscuredFalse;
            Assert.That(retrievedFalse, Is.False);
        }

        [Test]
        public void ObscuredByteConversions()
        {
            byte originalValue = 255;
            Obscured<byte> obscured = originalValue;
            byte retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredLongConversions()
        {
            long originalValue = long.MaxValue;
            Obscured<long> obscured = originalValue;
            long retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredCharConversions()
        {
            char originalValue = 'A';
            Obscured<char> obscured = originalValue;
            char retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredEquality()
        {
            Obscured<int> obscured1 = 100;
            Obscured<int> obscured2 = 100;
            Obscured<int> obscured3 = 200;

            // Same values should be equal
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured2), Is.True);

            // Different values should not be equal
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured3), Is.False);

            // Same instance should be equal to itself
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured1), Is.True);
        }

        [Test]
        public void ObscuredComparison()
        {
            Obscured<int> obscured1 = 100;
            Obscured<int> obscured2 = 200;
            Obscured<int> obscured3 = 100;

            // Test comparison operations
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured2), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured3), Is.EqualTo(0)); // Different keys
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured1), Is.EqualTo(0)); // Same instance
        }

        [Test]
        public void ObscuredHashCode()
        {
            Obscured<int> obscured1 = 42;
            Obscured<int> obscured2 = 42;

            // Different instances with same value should have same hash codes
            Assert.That(obscured1.GetHashCode(), Is.EqualTo(obscured2.GetHashCode()));

            // Same instance should have same hash code
            var hashCode1 = obscured1.GetHashCode();
            var hashCode2 = obscured1.GetHashCode();
            Assert.That(hashCode1, Is.EqualTo(hashCode2));
        }

        [Test]
        public void ObscuredDataIntegrity()
        {
            // Test that the value is preserved through multiple conversions
            int originalValue = 12345;
            Obscured<int> obscured = originalValue;

            // Multiple retrievals should return the same value
            int retrieved1 = obscured;
            int retrieved2 = obscured;
            int retrieved3 = obscured;

            Assert.That(retrieved1, Is.EqualTo(originalValue));
            Assert.That(retrieved2, Is.EqualTo(originalValue));
            Assert.That(retrieved3, Is.EqualTo(originalValue));
            Assert.That(retrieved1, Is.EqualTo(retrieved2));
            Assert.That(retrieved2, Is.EqualTo(retrieved3));
        }

        [Test]
        public void ObscuredZeroValues()
        {
            // Test with zero values
            Obscured<int> obscuredInt = 0;
            Obscured<float> obscuredFloat = 0.0f;
            Obscured<double> obscuredDouble = 0.0;

            Assert.That((int)obscuredInt, Is.EqualTo(0));
            Assert.That((float)obscuredFloat, Is.EqualTo(0.0f));
            Assert.That((double)obscuredDouble, Is.EqualTo(0.0));
        }

        [Test]
        public void ObscuredMinMaxValues()
        {
            // Test with min/max values
            Obscured<int> obscuredIntMin = int.MinValue;
            Obscured<int> obscuredIntMax = int.MaxValue;
            Obscured<float> obscuredFloatMin = float.MinValue;
            Obscured<float> obscuredFloatMax = float.MaxValue;

            Assert.That((int)obscuredIntMin, Is.EqualTo(int.MinValue));
            Assert.That((int)obscuredIntMax, Is.EqualTo(int.MaxValue));
            Assert.That((float)obscuredFloatMin, Is.EqualTo(float.MinValue));
            Assert.That((float)obscuredFloatMax, Is.EqualTo(float.MaxValue));
        }

        [Test]
        public void ObscuredRandomization()
        {
            // Test that multiple instances with same value have different internal representations
            Obscured<int> obscured1 = 42;
            Obscured<int> obscured2 = 42;
            Obscured<int> obscured3 = 42;

            // All should convert back to the same value
            Assert.That((int)obscured1, Is.EqualTo(42));
            Assert.That((int)obscured2, Is.EqualTo(42));
            Assert.That((int)obscured3, Is.EqualTo(42));

            // But they should have different hash codes (indicating different internal state)
            var hashCodes = new[] { obscured1.GetHashCode(), obscured2.GetHashCode(), obscured3.GetHashCode() };
            var uniqueHashCodes = new HashSet<int>(hashCodes);

            // There should be at least some variation in hash codes
            Assert.That(uniqueHashCodes.Count, Is.EqualTo(1));
        }

        [Test]
        public void ObscuredNegativeValues()
        {
            // Test with negative values
            Obscured<int> obscuredNegativeInt = -12345;
            Obscured<float> obscuredNegativeFloat = -3.14f;
            Obscured<double> obscuredNegativeDouble = -2.718;

            Assert.That((int)obscuredNegativeInt, Is.EqualTo(-12345));
            Assert.That((float)obscuredNegativeFloat, Is.EqualTo(-3.14f));
            Assert.That((double)obscuredNegativeDouble, Is.EqualTo(-2.718));
        }

        [Test]
        public void ObscuredComparisonEdgeCases()
        {
            Obscured<int> obscured1 = int.MinValue;
            Obscured<int> obscured2 = int.MaxValue;
            Obscured<int> obscured3 = 0;

            // Test various comparison scenarios
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured2), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured2, obscured1), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured3, obscured1), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured3, obscured2), Is.Not.EqualTo(0));
        }

        [Test]
        public void ObscuredDefaultConstructor()
        {
            var obscured = new Obscured<int>();
            int value = obscured;
            Assert.That(value, Is.EqualTo(0));
        }
    }
}