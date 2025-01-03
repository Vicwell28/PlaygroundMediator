namespace PlaygroundMediator.PipelineBehavior
{
    public interface IRequireAuthentication { }

    public interface IRequireAuthorization { }

    public interface IRequireValidation { }

    public interface IRequireLogging { }

    public interface IRequireCaching { }

    public interface IRequireRateLimiting { }

    public interface IRequireAuditLogging { }

    public interface IRequireExceptionHandling { }

    public interface IRequireResponseCaching { }

    public interface IRequireResponseCompression { }

    public interface IRequireResponseCachingInvalidation { }

    public interface IRequireResponseCompressionInvalidation { }

    public interface IRequireResponseCachingInvalidationOnUpdate { }

    public interface IRequireResponseCompressionInvalidationOnUpdate { }

    public interface IRequireResponseCompressionUpdate { }

    public interface IRequireResponseCachingUpdate { }

    public interface IRequireResponseCachingInvalidationOnDelete { }

    public interface IRequireResponseCompressionUpdateOnDelete { }

    public interface IRequireResponseCachingUpdateOnDelete { }

}
