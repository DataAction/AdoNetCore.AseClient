using System.Data;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    [TestFixture]
    [Category("quick")]
    public class AseParameterCollectionTests
    {
        [Test]
        public void ConstructParameterCollection_WithNoArgs_NoErrors()
        {
            var parameterCollection = new AseParameterCollection();

            Assert.IsEmpty(parameterCollection);
        }

        [Test]
        public void Add_ValidValue_IncreasesCount()
        {
            var parameterCollection = new AseParameterCollection {new AseParameter()};
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Remove_ValidReferenceValue_DecreasesCount()
        {
            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(new AseParameter());

            Assert.AreEqual(1, parameterCollection.Count);

            parameterCollection.Remove(parameter);

            Assert.IsEmpty(parameterCollection);
        }

        [Test]
        public void IndexOf_ValidReferenceValue_IsZero()
        {
            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(new AseParameter());

            Assert.AreEqual(1, parameterCollection.Count);

            var index = parameterCollection.IndexOf(parameter);

            Assert.AreEqual(0, index);
        }

        [Test]
        public void IndexOf_ValidName_IsZero()
        {
            const string parameterName = "@a_param";
            var parameterCollection = new AseParameterCollection { new AseParameter { ParameterName = parameterName } };

            Assert.AreEqual(1, parameterCollection.Count);

            var index = parameterCollection.IndexOf(parameterName);

            Assert.AreEqual(0, index);
        }

        [Test]
        public void Contains_ValidReferenceValue_IsTrue()
        {
            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(new AseParameter());

            Assert.AreEqual(1, parameterCollection.Count);

            var contains = parameterCollection.Contains(parameter);

            Assert.IsTrue(contains);
        }

        [Test]
        public void Contains_ValidName_IsTrue()
        {
            const string parameterName = "@a_param";
            var parameterCollection = new AseParameterCollection { new AseParameter { ParameterName = parameterName } };

            Assert.AreEqual(1, parameterCollection.Count);

            var contains = parameterCollection.Contains(parameterName);

            Assert.IsTrue(contains);
        }

        [Test]
        public void Clear_WithData_ResetsCount()
        {
            var parameterCollection = new AseParameterCollection {new AseParameter()};

            Assert.AreEqual(1, parameterCollection.Count);

            parameterCollection.Clear();

            Assert.IsEmpty(parameterCollection);
        }

        [Test]
        public void RemoveAt_ValidReferenceValue_DecreasesCount()
        {
            var parameterCollection = new AseParameterCollection { new AseParameter() };

            Assert.AreEqual(1, parameterCollection.Count);

            parameterCollection.RemoveAt(0);

            Assert.IsEmpty(parameterCollection);
        }

        [Test]
        public void RemoveAt_ValidName_DecreasesCount()
        {
            const string parameterName = "@a_param";
            var parameterCollection = new AseParameterCollection { new AseParameter {ParameterName = parameterName}};

            Assert.AreEqual(1, parameterCollection.Count);

            parameterCollection.RemoveAt(parameterName);

            Assert.IsEmpty(parameterCollection);
        }

        [Test]
        public void Insert_ValidReferenceValue_IncreasesCount()
        {
            var parameterCollection = new AseParameterCollection();
            parameterCollection.Insert(0, new AseParameter());

            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void CopyTo_ValidParameters_Copies()
        {
            var parameterCollection = new AseParameterCollection {new AseParameter()};
            var destination = new AseParameter[1];
            parameterCollection.CopyTo(destination, 0);

            Assert.AreEqual(parameterCollection.Count, destination.Length);
            Assert.IsNotNull(destination[0]);
        }

        [Test]
        public void Add_WithArgs1_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(parameterName, type);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Add_WithArgs2_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(parameterName, type,size);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(size, parameter.Size);
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Add_WithArgs3_NoErrors()
        {
            const string parameterName = "@a_parameter";
            const AseDbType type = AseDbType.VarChar;
            const int size = 256;
            const string sourceColumn = "a_column";

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(parameterName, type, size, sourceColumn);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(type, parameter.AseDbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(size, parameter.Size);
            Assert.AreEqual(sourceColumn, parameter.SourceColumn);
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Add_WithArgs4_NoErrors()
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

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(parameterName, type, size, parameterDirection, isNullable, precision, scale, sourceColumn, sourceVersion, value);

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
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Add_WithArgs5_NoErrors()
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

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(new AseParameter(parameterName, type, size, parameterDirection, isNullable, precision, scale, sourceColumn, sourceVersion, value));

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
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Add_WithStringValue_InfersType()
        {
            const string parameterName = "@a_parameter";
            const string value = "a value";

            var parameterCollection = new AseParameterCollection();
            var parameter = parameterCollection.Add(parameterName, value);

            Assert.AreEqual(parameterName, parameter.ParameterName);
            Assert.AreEqual(AseDbType.NVarChar, parameter.AseDbType);
            Assert.AreEqual(0, parameter.Size);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual(false, parameter.IsNullable);
            Assert.AreEqual(0, parameter.Precision);
            Assert.AreEqual(0, parameter.Scale);
            Assert.IsNull(parameter.SourceColumn);
            Assert.AreEqual(DataRowVersion.Default, parameter.SourceVersion);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(1, parameterCollection.Count);
        }

        [Test]
        public void Indexer_ReadByName_FindsValue()
        {
            const string parameterName = "@a_parameter";
            const string value = "a value";

            var parameterCollection = new AseParameterCollection();
            var expected = parameterCollection.Add(parameterName, value);

            Assert.AreEqual(expected, parameterCollection[parameterName]);
        }

        [Test]
        public void Indexer_WriteByName_OverwritesValue()
        {
            const string parameterName = "@a_parameter";
            const string value = "a value";

            var parameterCollection = new AseParameterCollection();
            var expected = parameterCollection.Add(parameterName, value);

            Assert.AreEqual(expected, parameterCollection[parameterName]);

            var other = new AseParameter(parameterName, 5);

            parameterCollection[parameterName] = other;

            Assert.AreEqual(other, parameterCollection[parameterName]);
        }
    }
}
