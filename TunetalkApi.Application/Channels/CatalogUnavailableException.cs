namespace TunetalkApi.Application.Channels;

public sealed class CatalogUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
