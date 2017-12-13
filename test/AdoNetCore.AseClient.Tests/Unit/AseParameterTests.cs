using System.Data;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    public class AseParameterTests
    {
        [Test]
        public void ConstructParameter_WithNoArgs_NoErrors()
        {
            var unused = new AseParameter();

            Assert.Pass();
        }

        [Test]
        public void ConstructParameter_WithArgs1_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;

            var parameter = new AseParameter(parameterName, type);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
        }

        [Test]
        public void ConstructParameter_WithArgs2_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;

            var parameter = new AseParameter(parameterName, type, size);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(size, parameter.Size);
        }

        [Test]
        public void ConstructParameter_WithArgs3_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;
            const string sourceColumn = "a_column";

            var parameter = new AseParameter(parameterName, type, size, sourceColumn);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(size, parameter.Size);
            Assert.AreEqual(sourceColumn, parameter.SourceColumn);
        }

        [Test]
        public void ConstructParameter_WithArgs4_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;
            const string sourceColumn = "a_column";
            const ParameterDirection parameterDirection = ParameterDirection.Output;
            const bool isNullable = true;
            const byte precision = 16;
            const byte scale = 24;
            const DataRowVersion sourceVersion = DataRowVersion.Default;
            const string value = "a value";

            var parameter = new AseParameter(parameterName, type, size, parameterDirection, isNullable, precision, scale, sourceColumn, sourceVersion, value);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(size, parameter.Size);
            Assert.AreEqual(parameterDirection, parameter.Direction);
            Assert.AreEqual(isNullable, parameter.IsNullable);
            Assert.AreEqual(precision, parameter.Precision);
            Assert.AreEqual(scale, parameter.Scale);
            Assert.AreEqual(sourceColumn, parameter.SourceColumn);
            Assert.AreEqual(sourceVersion, parameter.SourceVersion);
            Assert.AreEqual(value, parameter.Value);
        }

        [Test]
        public void ConstructParameter_WithStringValue_InfersType()
        {
            const string parameterName = "@a_parameter";
            const string value = "a value";

            var parameter = new AseParameter(parameterName, value);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(AseDbType.UniChar, parameter.AseDbType);
            Assert.AreEqual(value.Length, parameter.Size);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(false, parameter.IsNullable);
            Assert.AreEqual(0, parameter.Precision);
            Assert.AreEqual(0, parameter.Scale);
            Assert.IsNull(parameter.SourceColumn);
            Assert.AreEqual(DataRowVersion.Default, parameter.SourceVersion);
            Assert.AreEqual(value, parameter.Value);
        }

        [Test]
        public void ConstructParameter_ViaProperties_Success()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;
            const string sourceColumn = "a_column";
            const ParameterDirection parameterDirection = ParameterDirection.Output;
            const bool isNullable = true;
            const byte precision = 16;
            const byte scale = 24;
            const DataRowVersion sourceVersion = DataRowVersion.Default;
            const string value = "a value";

            var parameter = new AseParameter
            {
                ParameterName = parameterName,
                AseDbType = type,
                Size = size,
                Direction = parameterDirection,
                IsNullable = isNullable,
                Precision = precision,
                Scale = scale,
                SourceColumn = sourceColumn,
                SourceVersion = sourceVersion,
                Value = value
            };

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(size, parameter.Size);
            Assert.AreEqual(parameterDirection, parameter.Direction);
            Assert.AreEqual(isNullable, parameter.IsNullable);
            Assert.AreEqual(precision, parameter.Precision);
            Assert.AreEqual(scale, parameter.Scale);
            Assert.AreEqual(sourceColumn, parameter.SourceColumn);
            Assert.AreEqual(sourceVersion, parameter.SourceVersion);
            Assert.AreEqual(value, parameter.Value);
        }
    }
}
