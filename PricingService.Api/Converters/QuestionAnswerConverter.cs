using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PricingService.Api.Commands.Dto;

namespace PricingService.Api.Converters;

internal class QuestionAnswerConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(QuestionAnswer).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var target = Create(jsonObject);
        serializer.Populate(jsonObject.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is QuestionAnswer questionAnswer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("questionCode");
            serializer.Serialize(writer, questionAnswer.QuestionCode);
            writer.WritePropertyName("questionType");
            serializer.Serialize(writer, questionAnswer.QuestionType);
            writer.WritePropertyName("answer");
            serializer.Serialize(writer, questionAnswer.GetAnswer());
            writer.WriteEndObject();
            return;
        }

        throw new JsonSerializationException($"Expected {nameof(QuestionAnswer)} value.");
    }

    private static QuestionAnswer Create(JObject jsonObject)
    {
        var typeName = Enum.Parse<QuestionType>(RequiredString(jsonObject, "questionType"), true);
        switch (typeName)
        {
            case QuestionType.Text:
                return new TextQuestionAnswer
                {
                    QuestionCode = RequiredString(jsonObject, "questionCode"),
                    Answer = RequiredString(jsonObject, "answer")
                };
            case QuestionType.Number:
                return new NumericQuestionAnswer
                {
                    QuestionCode = RequiredString(jsonObject, "questionCode"),
                    Answer = RequiredToken(jsonObject, "answer").Value<decimal>()
                };
            case QuestionType.Choice:
                return new ChoiceQuestionAnswer
                {
                    QuestionCode = RequiredString(jsonObject, "questionCode"),
                    Answer = RequiredString(jsonObject, "answer")
                };
            default:
                throw new ApplicationException($"Unexpected question type {typeName}");
        }
    }

    private static JToken RequiredToken(JObject jsonObject, string propertyName)
    {
        return jsonObject[propertyName]
            ?? throw new JsonSerializationException($"Property '{propertyName}' is required.");
    }

    private static string RequiredString(JObject jsonObject, string propertyName)
    {
        return RequiredToken(jsonObject, propertyName).Value<string>()
            ?? throw new JsonSerializationException($"Property '{propertyName}' must be a string.");
    }
}
