using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.TileSettings;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.Serialization
{
    internal static class PowerPlannerJson
    {
        public static string Serialize<T>(T value)
        {
            JsonTypeInfo typeInfo = PowerPlannerJsonContext.Default.GetTypeInfo(typeof(T));
            if (typeInfo == null)
            {
                throw new NotSupportedException($"JSON type metadata is not registered for {typeof(T).FullName}.");
            }

            return JsonSerializer.Serialize(value, typeInfo);
        }

        public static T Deserialize<T>(string json)
        {
            JsonTypeInfo typeInfo = PowerPlannerJsonContext.Default.GetTypeInfo(typeof(T));
            if (typeInfo == null)
            {
                throw new NotSupportedException($"JSON type metadata is not registered for {typeof(T).FullName}.");
            }

            return (T)JsonSerializer.Deserialize(json, typeInfo);
        }
    }

    [JsonSourceGenerationOptions(IncludeFields = true, PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(CreateAccountRequest), TypeInfoPropertyName = "CreateAccountRequestTypeInfo")]
    [JsonSerializable(typeof(CreateAccountResponse), TypeInfoPropertyName = "CreateAccountResponseTypeInfo")]
    [JsonSerializable(typeof(ForgotUsernameRequest), TypeInfoPropertyName = "ForgotUsernameRequestTypeInfo")]
    [JsonSerializable(typeof(ForgotUsernameResponse), TypeInfoPropertyName = "ForgotUsernameResponseTypeInfo")]
    [JsonSerializable(typeof(ResetPasswordRequest), TypeInfoPropertyName = "ResetPasswordRequestTypeInfo")]
    [JsonSerializable(typeof(ResetPasswordResponse), TypeInfoPropertyName = "ResetPasswordResponseTypeInfo")]
    [JsonSerializable(typeof(UndeleteItemRequest), TypeInfoPropertyName = "UndeleteItemRequestTypeInfo")]
    [JsonSerializable(typeof(PlainResponse), TypeInfoPropertyName = "PlainResponseTypeInfo")]
    [JsonSerializable(typeof(SyncRequest), TypeInfoPropertyName = "SyncRequestTypeInfo")]
    [JsonSerializable(typeof(SyncResponse), TypeInfoPropertyName = "SyncResponseTypeInfo")]
    [JsonSerializable(typeof(SyncSettingsRequest), TypeInfoPropertyName = "SyncSettingsRequestTypeInfo")]
    [JsonSerializable(typeof(SyncSettingsResponse), TypeInfoPropertyName = "SyncSettingsResponseTypeInfo")]
    [JsonSerializable(typeof(AddPremiumAccountDurationRequest), TypeInfoPropertyName = "AddPremiumAccountDurationRequestTypeInfo")]
    [JsonSerializable(typeof(AddPremiumAccountDurationResponse), TypeInfoPropertyName = "AddPremiumAccountDurationResponseTypeInfo")]
    [JsonSerializable(typeof(PartialLoginRequest), TypeInfoPropertyName = "PartialLoginRequestTypeInfo")]
    [JsonSerializable(typeof(GetDeletedYearsAndSemestersResponse), TypeInfoPropertyName = "GetDeletedYearsAndSemestersResponseTypeInfo")]
    [JsonSerializable(typeof(GetEmailRequest), TypeInfoPropertyName = "GetEmailRequestTypeInfo")]
    [JsonSerializable(typeof(GetEmailResponse), TypeInfoPropertyName = "GetEmailResponseTypeInfo")]
    [JsonSerializable(typeof(ChangeEmailRequest), TypeInfoPropertyName = "ChangeEmailRequestTypeInfo")]
    [JsonSerializable(typeof(ChangeEmailResponse), TypeInfoPropertyName = "ChangeEmailResponseTypeInfo")]
    [JsonSerializable(typeof(ChangeUsernameRequest), TypeInfoPropertyName = "ChangeUsernameRequestTypeInfo")]
    [JsonSerializable(typeof(ChangeUsernameResponse), TypeInfoPropertyName = "ChangeUsernameResponseTypeInfo")]
    [JsonSerializable(typeof(DeleteAccountRequest), TypeInfoPropertyName = "DeleteAccountRequestTypeInfo")]
    [JsonSerializable(typeof(DeleteAccountResponse), TypeInfoPropertyName = "DeleteAccountResponseTypeInfo")]
    [JsonSerializable(typeof(DeleteDevicesRequest), TypeInfoPropertyName = "DeleteDevicesRequestTypeInfo")]
    [JsonSerializable(typeof(DeleteDevicesResponse), TypeInfoPropertyName = "DeleteDevicesResponseTypeInfo")]
    [JsonSerializable(typeof(UpdatedItems), TypeInfoPropertyName = "UpdatedItemsTypeInfo")]
    [JsonSerializable(typeof(UploadImageResponse), TypeInfoPropertyName = "UploadImageResponseTypeInfo")]
    [JsonSerializable(typeof(SyncedSettings), TypeInfoPropertyName = "SyncedSettingsTypeInfo")]
    [JsonSerializable(typeof(Dictionary<Guid, ChangedPropertiesOfDataItem>), TypeInfoPropertyName = "ChangedItemsTypeInfo")]
    [JsonSerializable(typeof(GradeScale[]), TypeInfoPropertyName = "GradeScaleArrayTypeInfo")]
    [JsonSerializable(typeof(PhoneNumber[]), TypeInfoPropertyName = "PhoneNumberArrayTypeInfo")]
    [JsonSerializable(typeof(EmailAddress[]), TypeInfoPropertyName = "EmailAddressArrayTypeInfo")]
    [JsonSerializable(typeof(PostalAddress[]), TypeInfoPropertyName = "PostalAddressArrayTypeInfo")]
    [JsonSerializable(typeof(string[]), TypeInfoPropertyName = "StringArrayTypeInfo")]
    [JsonSerializable(typeof(ClassTileSettings), TypeInfoPropertyName = "ClassTileSettingsTypeInfo")]
    [JsonSerializable(typeof(Schedule.Type), TypeInfoPropertyName = "ScheduleTypeInfo")]
    [JsonSerializable(typeof(EmailAddress.Type), TypeInfoPropertyName = "EmailAddressTypeInfo")]
    [JsonSerializable(typeof(PostalAddress.Type), TypeInfoPropertyName = "PostalAddressTypeInfo")]
    [JsonSerializable(typeof(PhoneNumber.Type), TypeInfoPropertyName = "PhoneNumberTypeInfo")]
    internal partial class PowerPlannerJsonContext : JsonSerializerContext
    {
    }
}