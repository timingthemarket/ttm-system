using System.Text.Json.Serialization;

namespace boersdata_raw.DataAccess.Models.BoersDataApi;

public record BoersDataTranslationMetadata(
    [property: JsonPropertyName("nameSv")] string NameSv,
    [property: JsonPropertyName("nameEn")] string NameEn,
    [property: JsonPropertyName("translationKey")] string TranslationKey
);

public record BoersDataTranslations(
    [property: JsonPropertyName("translationMetadatas")] IReadOnlyList<BoersDataTranslationMetadata> TranslationMetadatas
);