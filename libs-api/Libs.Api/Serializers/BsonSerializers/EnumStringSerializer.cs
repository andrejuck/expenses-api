using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Libs.Api.Serializers;

public class EnumStringSerializer<TEnum> : SerializerBase<TEnum> where TEnum : struct, Enum
{
    public override TEnum Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        switch (reader.CurrentBsonType)
        {
            case BsonType.String:
                var enumString = reader.ReadString();
                if (Enum.TryParse(enumString, out TEnum resultByString))
                {
                    return resultByString;
                }
                break;

            case BsonType.Int32:
                var enumInt = reader.ReadInt32();
                if (Enum.IsDefined(typeof(TEnum), enumInt))
                {
                    return (TEnum)Enum.ToObject(typeof(TEnum), enumInt);
                }
                break;

            default:
                throw new BsonSerializationException($"Invalid type for {typeof(TEnum).Name}: {reader.CurrentBsonType}");
        }

        throw new BsonSerializationException($"Invalid type for {typeof(TEnum).Name}");
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnum value)
    {
        context.Writer.WriteString(value.ToString());
    }
}
