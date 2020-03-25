namespace Validot.Tests.Unit.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using NSubstitute;

    using Validot.Errors;
    using Validot.Results;

    using Xunit;

    public class ValidationResultTests
    {
        [Fact]
        public void Should_Initialize()
        {
            _ = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());
        }

        [Fact]
        public void Details_Should_NotBeNull()
        {
            var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

            validationResult.Details.Should().NotBeNull();
        }

        [Fact]
        public void Details_Should_BeSelf()
        {
            var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

            validationResult.Details.Should().BeSameAs(validationResult);
        }

        [Fact]
        public void IsValid_Should_BeTrue_When_NoErrors()
        {
            var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

            validationResult.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_Should_BeFalse_When_AnyError()
        {
            var resultErrors = new Dictionary<string, List<int>>();
            resultErrors.Add("path", new List<int>() { 1 });

            var validationResult = new ValidationResult(resultErrors, new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

            validationResult.IsValid.Should().BeFalse();
        }

        [Fact]
        public void NoErrorsResult_Should_BeResultWithoutErrors()
        {
            ValidationResult.NoErrorsResult.IsValid.Should().BeTrue();
            ValidationResult.NoErrorsResult.PathsWithErrors.Should().BeEmpty();
            ValidationResult.NoErrorsResult.RegisteredTranslationsNames.Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetErrorCodes().Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetErrorMessages().Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetRawErrors().Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetTranslation(null).Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetTranslation("English").Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetErrorMessages().Should().BeEmpty();
            ValidationResult.NoErrorsResult.GetErrorMessages("English").Should().BeEmpty();
        }

        public static IEnumerable<object[]> PathWithErrors_Should_ReturnAllPaths_Data()
        {
            yield return new object[]
            {
                new Dictionary<string, List<int>>(),
                Array.Empty<string>()
            };

            yield return new object[]
            {
                new Dictionary<string, List<int>>()
                {
                    ["test1"] = new List<int>(),
                },
                new[] { "test1" }
            };

            yield return new object[]
            {
                new Dictionary<string, List<int>>()
                {
                    ["test1"] = new List<int>(),
                    ["test2"] = new List<int>(),
                    ["nested.test3"] = new List<int>()
                },
                new[] { "test1", "test2", "nested.test3" }
            };
        }

        [Theory]
        [MemberData(nameof(PathWithErrors_Should_ReturnAllPaths_Data))]
        public void PathWithErrors_Should_ReturnAllPaths(Dictionary<string, List<int>> resultsErrors, IReadOnlyList<string> expectedPaths)
        {
            var validationResult = new ValidationResult(resultsErrors, new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

            validationResult.PathsWithErrors.Should().NotBeNull();
            validationResult.PathsWithErrors.Should().HaveCount(expectedPaths.Count);

            foreach (var expectedPath in expectedPaths)
            {
                validationResult.PathsWithErrors.Should().Contain(expectedPath);
            }
        }

        public class RegisteredTranslationsNames
        {
            [Fact]
            public void Should_Return_TranslationNames_FromMessageService()
            {
                var messagesService = Substitute.For<IMessagesService>();

                var translationNames = new[]
                {
                    "translation1",
                    "translation2"
                };

                messagesService.TranslationsNames.Returns(translationNames);

                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), messagesService);

                validationResult.RegisteredTranslationsNames.Should().BeSameAs(translationNames);
            }

            [Fact]
            public void Should_Return_EmptyTranslationNames_When_NullTranslationName_InMessageService()
            {
                var messagesService = Substitute.For<IMessagesService>();

                messagesService.TranslationsNames.Returns(null as IReadOnlyList<string>);

                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), null);

                validationResult.RegisteredTranslationsNames.Should().BeEmpty();
            }

            [Fact]
            public void Should_Return_EmptyTranslationNames_When_NullMessageService()
            {
                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), null);

                validationResult.RegisteredTranslationsNames.Should().BeEmpty();
            }
        }

        public class GetErrorMessages
        {
            [Fact]
            public void Should_Return_ErrorMessages_FromMessageService_WithDefaultTranslation()
            {
                var messagesService = Substitute.For<IMessagesService>();

                var errorMessages = new Dictionary<string, IReadOnlyList<string>>
                {
                    ["path1"] = new[] { "message11" },
                    ["path2"] = new[] { "message12", "message22" }
                };

                var resultErrors = new Dictionary<string, List<int>>()
                {
                    ["path1"] = new List<int>() { 1 }
                };

                messagesService.GetErrorsMessages(Arg.Is<Dictionary<string, List<int>>>(a => ReferenceEquals(a, resultErrors)), Arg.Is<string>(null as string)).Returns(errorMessages);

                var validationResult = new ValidationResult(resultErrors, new Dictionary<int, IError>(), messagesService);

                var resultErrorMessages = validationResult.GetErrorMessages();

                messagesService.Received(1).GetErrorsMessages(Arg.Is<Dictionary<string, List<int>>>(a => ReferenceEquals(a, resultErrors)), Arg.Is<string>(null as string));
                messagesService.ReceivedWithAnyArgs(1).GetErrorsMessages(default);

                resultErrorMessages.Should().BeSameAs(errorMessages);
            }

            [Fact]
            public void Should_Return_ErrorMessages_FromMessageService_WithSpecifiedTranslation()
            {
                var messagesService = Substitute.For<IMessagesService>();

                var errorMessages1 = new Dictionary<string, IReadOnlyList<string>>
                {
                    ["path1"] = new[] { "message11" },
                    ["path2"] = new[] { "message12", "message22" }
                };

                var errorMessages2 = new Dictionary<string, IReadOnlyList<string>>
                {
                    ["path1"] = new[] { "MESSAGE11" },
                    ["path2"] = new[] { "MESSAGE12", "MESSAGE22" }
                };

                var resultErrors = new Dictionary<string, List<int>>()
                {
                    ["path1"] = new List<int>() { 1 }
                };

                messagesService.GetErrorsMessages(Arg.Is<Dictionary<string, List<int>>>(a => ReferenceEquals(a, resultErrors)), Arg.Is<string>("translation1")).Returns(errorMessages1);
                messagesService.GetErrorsMessages(Arg.Is<Dictionary<string, List<int>>>(a => ReferenceEquals(a, resultErrors)), Arg.Is<string>("translation2")).Returns(errorMessages2);

                var validationResult = new ValidationResult(resultErrors, new Dictionary<int, IError>(), messagesService);

                var resultErrorMessages = validationResult.GetErrorMessages("translation2");

                messagesService.Received(1).GetErrorsMessages(Arg.Is<Dictionary<string, List<int>>>(a => ReferenceEquals(a, resultErrors)), Arg.Is<string>("translation2"));
                messagesService.ReceivedWithAnyArgs(1).GetErrorsMessages(default);

                resultErrorMessages.Should().BeSameAs(errorMessages2);
            }

            [Fact]
            public void Should_Return_EmptyErrorMessages_When_Valid()
            {
                var messagesService = Substitute.For<IMessagesService>();

                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), null);

                var resultErrorMessages = validationResult.GetErrorMessages();

                messagesService.DidNotReceiveWithAnyArgs().GetErrorsMessages(default);

                resultErrorMessages.Should().NotBeNull();
                resultErrorMessages.Should().BeEmpty();
            }
        }

        public class GetTranslation
        {
            [Fact]
            public void Should_Return_Translation_FromMessageService()
            {
                var messagesService = Substitute.For<IMessagesService>();

                var translation = new Dictionary<string, string>()
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2",
                };

                messagesService.GetTranslation(Arg.Is("translationName1")).Returns(translation);

                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), messagesService);

                var resultTranslation = validationResult.GetTranslation("translationName1");

                messagesService.Received(1).GetTranslation(Arg.Is("translationName1"));
                messagesService.ReceivedWithAnyArgs(1).GetTranslation(default);

                resultTranslation.Should().BeSameAs(translation);
            }

            [Fact]
            public void Should_Return_EmptyTranslation_When_NullMessageService()
            {
                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), null);

                var resultTranslation = validationResult.GetTranslation("translationName1");

                resultTranslation.Should().BeEmpty();
            }
        }

        public class GetRawErrors
        {
            [Fact]
            public void Should_ReturnPathsWithRawErrors()
            {
                var resultsErrors = new Dictionary<string, List<int>>()
                {
                    ["test1"] = new List<int>() { 1, 2, 3 },
                    ["test2"] = new List<int>() { 2, 4 },
                };

                var errorsRegistry = new Dictionary<int, IError>()
                {
                    [1] = new Error(),
                    [2] = new Error(),
                    [3] = new Error(),
                    [4] = new Error(),
                };

                var validationResult = new ValidationResult(resultsErrors, errorsRegistry, Substitute.For<IMessagesService>());

                var rawErrors = validationResult.GetRawErrors();

                rawErrors.Should().NotBeNull();

                rawErrors.Keys.Should().HaveCount(2);

                rawErrors["test1"].Should().HaveCount(3);
                rawErrors["test1"].Should().Contain(x => ReferenceEquals(x, errorsRegistry[1]));
                rawErrors["test1"].Should().Contain(x => ReferenceEquals(x, errorsRegistry[2]));
                rawErrors["test1"].Should().Contain(x => ReferenceEquals(x, errorsRegistry[3]));

                rawErrors["test2"].Should().HaveCount(2);
                rawErrors["test2"].Should().Contain(x => ReferenceEquals(x, errorsRegistry[2]));
                rawErrors["test2"].Should().Contain(x => ReferenceEquals(x, errorsRegistry[4]));
            }

            public static IEnumerable<object[]> Should_ReturnPathsWithRawErrors_MoreExamples_Data()
            {
                var errorsRegistry = new Dictionary<int, IError>()
                {
                    [1] = new Error(),
                    [2] = new Error(),
                    [3] = new Error(),
                    [4] = new Error(),
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                    },
                    errorsRegistry,
                    new Dictionary<string, IReadOnlyList<IError>>()
                    {
                        ["test1"] = new[] { errorsRegistry[1] }
                    },
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        [""] = new List<int>() { 4 },
                    },
                    errorsRegistry,
                    new Dictionary<string, IReadOnlyList<IError>>()
                    {
                        [""] = new[] { errorsRegistry[4] }
                    },
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>(),
                    },
                    errorsRegistry,
                    new Dictionary<string, IReadOnlyList<IError>>()
                    {
                        ["test1"] = new IError[] { },
                    },
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                        ["test2"] = new List<int>() { 2 },
                        ["test3"] = new List<int>() { 3 },
                        ["test4"] = new List<int>() { 4 },
                    },
                    errorsRegistry,
                    new Dictionary<string, IReadOnlyList<IError>>()
                    {
                        ["test1"] = new[] { errorsRegistry[1] },
                        ["test2"] = new[] { errorsRegistry[2] },
                        ["test3"] = new[] { errorsRegistry[3] },
                        ["test4"] = new[] { errorsRegistry[4] },
                    },
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                        ["test2"] = new List<int>() { 1, 2 },
                        ["test3"] = new List<int>() { 1, 3 },
                        ["test4"] = new List<int>() { 2, 3, 4 },
                    },
                    errorsRegistry,
                    new Dictionary<string, IReadOnlyList<IError>>()
                    {
                        ["test1"] = new[] { errorsRegistry[1] },
                        ["test2"] = new[] { errorsRegistry[1], errorsRegistry[2] },
                        ["test3"] = new[] { errorsRegistry[1], errorsRegistry[3] },
                        ["test4"] = new[] { errorsRegistry[2], errorsRegistry[3], errorsRegistry[4] },
                    },
                };
            }

            [Theory]
            [MemberData(nameof(Should_ReturnPathsWithRawErrors_MoreExamples_Data))]
            public void Should_ReturnPathsWithRawErrors_MoreExamples(Dictionary<string, List<int>> resultsErrors, Dictionary<int, IError> errorsRegistry, IReadOnlyDictionary<string, IReadOnlyList<IError>> expectedErrors)
            {
                var validationResult = new ValidationResult(resultsErrors, errorsRegistry, Substitute.For<IMessagesService>());

                var rawErrors = validationResult.GetRawErrors();

                rawErrors.Should().NotBeNull();

                rawErrors.Keys.Should().HaveCount(resultsErrors.Count);

                foreach (var expectedErrorsPair in expectedErrors)
                {
                    rawErrors.Keys.Should().Contain(expectedErrorsPair.Key);
                    rawErrors[expectedErrorsPair.Key].Should().HaveCount(expectedErrorsPair.Value.Count);

                    foreach (var error in expectedErrorsPair.Value)
                    {
                        rawErrors[expectedErrorsPair.Key].Should().Contain(x => ReferenceEquals(x, error));
                    }
                }
            }

            [Fact]
            public void Should_ReturnEmptyDictionary_When_Valid()
            {
                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

                var rawErrors = validationResult.GetRawErrors();

                rawErrors.Should().NotBeNull();
                rawErrors.Should().BeEmpty();
            }
        }

        public class GetErrorCodes
        {
            [Fact]
            public void Should_ReturnAllErrorCodesFromErrors()
            {
                var resultsErrors = new Dictionary<string, List<int>>()
                {
                    ["test1"] = new List<int>() { 1, 2, 3 },
                    ["test2"] = new List<int>() { 2, 4 },
                };

                var errorsRegistry = new Dictionary<int, IError>()
                {
                    [1] = new Error()
                    {
                        Codes = new[] { "CODE1", }
                    },
                    [2] = new Error()
                    {
                        Codes = new[] { "CODE2", }
                    },
                    [3] = new Error()
                    {
                        Codes = new[] { "CODE3", }
                    },
                    [4] = new Error()
                    {
                        Codes = new[] { "CODE41", "CODE42" }
                    },
                };

                var validationResult = new ValidationResult(resultsErrors, errorsRegistry, Substitute.For<IMessagesService>());

                var errorCodes = validationResult.GetErrorCodes();

                errorCodes.Should().NotBeNull();

                errorCodes.Should().HaveCount(6);

                errorCodes.Should().Contain("CODE1");
                errorCodes.Should().Contain("CODE2");
                errorCodes.Should().Contain("CODE3");
                errorCodes.Should().Contain("CODE41");
                errorCodes.Should().Contain("CODE42");

                errorCodes.Where(c => c == "CODE2").Should().HaveCount(2);
            }

            public static IEnumerable<object[]> Should_ReturnAllErrorCodesFromErrors_MoreExamples_Data()
            {
                var errorsRegistry = new Dictionary<int, IError>()
                {
                    [1] = new Error()
                    {
                        Codes = new[] { "CODE1", }
                    },
                    [2] = new Error()
                    {
                        Codes = new[] { "CODE2", }
                    },
                    [3] = new Error()
                    {
                        Codes = new[] { "CODE3", }
                    },
                    [4] = new Error()
                    {
                        Codes = new[] { "CODE41", "CODE42" }
                    },
                    [5] = new Error()
                    {
                    },
                    [6] = new Error()
                    {
                        Codes = new[] { "CODE61", "CODE62", "CODE63" }
                    },
                    [10] = new Error()
                    {
                        Codes = new[] { "CODE1", "CODE2" }
                    },
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE1"] = 1,
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1, 1, 1 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE1"] = 3,
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                        ["test2"] = new List<int>() { 1 },
                        ["test3"] = new List<int>() { 1 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE1"] = 3,
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 1 },
                        ["test2"] = new List<int>() { 2, 4 },
                        ["test3"] = new List<int>() { 3, 4 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE1"] = 1,
                        ["CODE2"] = 1,
                        ["CODE3"] = 1,
                        ["CODE41"] = 2,
                        ["CODE42"] = 2,
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 5 },
                        ["test2"] = new List<int>() { 2, 5 },
                        ["test3"] = new List<int>() { 3, 5 },
                        ["test4"] = new List<int>() { 6, 5 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE2"] = 1,
                        ["CODE3"] = 1,
                        ["CODE61"] = 1,
                        ["CODE62"] = 1,
                        ["CODE63"] = 1,
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, List<int>>()
                    {
                        ["test1"] = new List<int>() { 5, 10 },
                        ["test2"] = new List<int>() { 2, 5 },
                        ["test3"] = new List<int>() { 3, 6 },
                        ["test4"] = new List<int>() { 5 },
                        ["test5"] = new List<int>() { 5 },
                    },
                    errorsRegistry,
                    new Dictionary<string, int>()
                    {
                        ["CODE1"] = 1,
                        ["CODE2"] = 2,
                        ["CODE3"] = 1,
                        ["CODE61"] = 1,
                        ["CODE62"] = 1,
                        ["CODE63"] = 1,
                    }
                };
            }

            [Theory]
            [MemberData(nameof(Should_ReturnAllErrorCodesFromErrors_MoreExamples_Data))]
            public void Should_ReturnAllErrorCodesFromErrors_MoreExamples(Dictionary<string, List<int>> resultsErrors, Dictionary<int, IError> errorsRegistry, Dictionary<string, int> expectedCodesWithCount)
            {
                var validationResult = new ValidationResult(resultsErrors, errorsRegistry, Substitute.For<IMessagesService>());

                var errorCodes = validationResult.GetErrorCodes();

                errorCodes.Should().NotBeNull();

                var expectedCount = 0;

                foreach (var expectedPair in expectedCodesWithCount)
                {
                    expectedCount += expectedPair.Value;
                }

                errorCodes.Should().HaveCount(expectedCount);

                foreach (var expectedErrorsPair in expectedCodesWithCount)
                {
                    errorCodes.Should().Contain(expectedErrorsPair.Key);
                    errorCodes.Where(c => c == expectedErrorsPair.Key).Should().HaveCount(expectedErrorsPair.Value, because: $"Invalid amount of code {expectedErrorsPair.Key}");
                }
            }

            [Fact]
            public void Should_ReturnEmptyList_When_Valid()
            {
                var validationResult = new ValidationResult(new Dictionary<string, List<int>>(), new Dictionary<int, IError>(), Substitute.For<IMessagesService>());

                var errorCodes = validationResult.GetErrorCodes();

                errorCodes.Should().NotBeNull();
                errorCodes.Should().BeEmpty();
            }
        }
    }
}