using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Any;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Tests.Visitors
{
    [TestClass]
    public class CharTypeVisitorTests
    {
        private VisitorCollection _visitorCollection;
        private IVisitor _visitor;
        private IVisitor _nullableVisitor;
        private NamingStrategy _strategy;

        [TestInitialize]
        public void Init()
        {
            this._visitorCollection = VisitorCollection.CreateInstance();
            this._visitor = new CharTypeVisitor(this._visitorCollection);
            this._nullableVisitor = new NullableObjectTypeVisitor(this._visitorCollection);
            this._strategy = new CamelCaseNamingStrategy();
        }

        [DataTestMethod]
        [DataRow(typeof(char), false)]
        public void Given_Type_When_IsNavigatable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsNavigatable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), false)]
        [DataRow(typeof(Nullable<char>), false)]
        public void Given_NullableType_When_IsNavigatable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._nullableVisitor.IsNavigatable(type);

            result.Should().Be(expected);
        }


        [DataTestMethod]
        [DataRow(typeof(char), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), true)]
        [DataRow(typeof(Nullable<char>), true)]
        [DataRow(typeof(int), false)] 
        public void Given_NullableType_When_IsVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._nullableVisitor.IsVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsParameterVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsParameterVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), true)]
        [DataRow(typeof(Nullable<char>), true)]
        [DataRow(typeof(int), false)]
        public void Given_NullableType_When_IsParameterVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._nullableVisitor.IsParameterVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char), true)]
        [DataRow(typeof(int), false)]
        public void Given_Type_When_IsPayloadVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._visitor.IsPayloadVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), true)]
        [DataRow(typeof(Nullable<char>), true)]
        [DataRow(typeof(int), false)]
        public void Given_NullableType_When_IsPayloadVisitable_Invoked_Then_It_Should_Return_Result(Type type, bool expected)
        {
            var result = this._nullableVisitor.IsPayloadVisitable(type);

            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("string", null)]
        public void Given_Type_When_Visit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat)
        {
            var name = "hello";
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, typeof(char));

            this._visitor.Visit(acceptor, type, this._strategy);

            acceptor.Schemas.Should().ContainKey(name);
            acceptor.Schemas[name].Type.Should().Be(dataType);
            acceptor.Schemas[name].Format.Should().Be(dataFormat);
            acceptor.Schemas[name].MaxLength.Should().Be(1);
            acceptor.Schemas[name].MinLength.Should().Be(1);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), "string", null, true)]
        [DataRow(typeof(Nullable<char>), "string", null, true)]
        public void Given_NullableType_When_Visit_Invoked_Then_It_Should_Return_Result(Type objectType, string dataType, string dataFormat, bool schemaNullable)
        {
            var name = "hello";
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, objectType);

            this._nullableVisitor.Visit(acceptor, type, this._strategy);

            acceptor.Schemas.Should().ContainKey(name);
            acceptor.Schemas[name].Type.Should().Be(dataType);
            acceptor.Schemas[name].Format.Should().Be(dataFormat);
            acceptor.Schemas[name].Nullable.Should().Be(schemaNullable);
            acceptor.Schemas[name].MaxLength.Should().Be(1);
            acceptor.Schemas[name].MinLength.Should().Be(1);
        }

        [DataTestMethod]
        [DataRow("hello", "lorem ipsum")]
        public void Given_OpenApiPropertyAttribute_When_Visit_Invoked_Then_It_Should_Return_Result(string name, string description)
        {
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, typeof(char));
            var attribute = new OpenApiPropertyAttribute() { Description = description };

            this._visitor.Visit(acceptor, type, this._strategy, attribute);

            acceptor.Schemas[name].Nullable.Should().Be(false);
            acceptor.Schemas[name].Default.Should().BeNull();
            acceptor.Schemas[name].Description.Should().Be(description);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), true, "hello", "lorem ipsum")]
        [DataRow(typeof(Nullable<char>), true, "hello", "lorem ipsum")]
        [DataRow(typeof(char?), false, "hello", "lorem ipsum")]
        [DataRow(typeof(Nullable<char>), false, "hello", "lorem ipsum")]
        public void Given_OpenApiPropertyAttribute_When_Visit_Invoked_Then_It_Should_Return_Result(Type objectType, bool nullable, string name, string description)
        {
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, objectType);
            var attribute = new OpenApiPropertyAttribute() { Nullable = nullable,  Description = description };

            this._nullableVisitor.Visit(acceptor, type, this._strategy, attribute);

            acceptor.Schemas[name].Nullable.Should().Be(false);
            acceptor.Schemas[name].Default.Should().BeNull();
            acceptor.Schemas[name].Description.Should().Be(description);
        }

        [DataTestMethod]
        [DataRow("hello", OpenApiVisibilityType.Advanced)]
        [DataRow("hello", OpenApiVisibilityType.Important)]
        [DataRow("hello", OpenApiVisibilityType.Internal)]
        public void Given_OpenApiSchemaVisibilityAttribute_When_Visit_Invoked_Then_It_Should_Return_Result(string name, OpenApiVisibilityType visibility)
        {
            var acceptor = new OpenApiSchemaAcceptor();
            var type = new KeyValuePair<string, Type>(name, typeof(char));
            var attribute = new OpenApiSchemaVisibilityAttribute(visibility);

            this._visitor.Visit(acceptor, type, this._strategy, attribute);

            acceptor.Schemas[name].Extensions.Should().ContainKey("x-ms-visibility");
            acceptor.Schemas[name].Extensions["x-ms-visibility"].Should().BeOfType<OpenApiString>();
            (acceptor.Schemas[name].Extensions["x-ms-visibility"] as OpenApiString).Value.Should().Be(visibility.ToDisplayName(this._strategy));
        }

        [DataTestMethod]
        [DataRow("string", null)]
        public void Given_Type_When_ParameterVisit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat)
        {
            var result = this._visitor.ParameterVisit(typeof(char), this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);

        }

        [DataTestMethod]
        [DataRow(typeof(char?), "string", null, true)]
        [DataRow(typeof(Nullable<char>), "string", null, true)]
        public void Given_NullableType_When_ParameterVisit_Invoked_Then_It_Should_Return_Result(Type objectType, string dataType, string dataFormat, bool schemaNullable)
        {
            var result = this._nullableVisitor.ParameterVisit(objectType, this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);
            result.Nullable.Should().Be(schemaNullable);
        }

        [DataTestMethod]
        [DataRow("string", null)]
        public void Given_Type_When_PayloadVisit_Invoked_Then_It_Should_Return_Result(string dataType, string dataFormat)
        {
            var result = this._visitor.PayloadVisit(typeof(char), this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);
        }

        [DataTestMethod]
        [DataRow(typeof(char?), "string", null, true)]
        [DataRow(typeof(Nullable<char>), "string", null, true)]
        public void Given_Type_When_PayloadVisit_Invoked_Then_It_Should_Return_Result(Type objectType, string dataType, string dataFormat, bool schemaNullable)
        {
            var result = this._nullableVisitor.PayloadVisit(objectType, this._strategy);

            result.Type.Should().Be(dataType);
            result.Format.Should().Be(dataFormat);
            result.Nullable.Should().Be(schemaNullable);
        }
    }
}