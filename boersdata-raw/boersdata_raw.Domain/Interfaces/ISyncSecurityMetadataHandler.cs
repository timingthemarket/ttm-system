namespace boersdata_raw.Domain.Interfaces;

public interface ISyncSecurityMetadataHandler
{
    Task HandleSyncMetadata();
}