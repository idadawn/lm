using System.ComponentModel;

namespace Poxiao.Extras.CollectiveOAuth.Enums;

/// <summary>
/// 授权响应状态.
/// </summary>
public enum AuthResponseStatus
{
    /// <summary>
    /// 2000：正常；
    /// other：调用异常，具体异常内容见<c>msg</c>.
    /// </summary>
    [Description("Success")]
    SUCCESS = 2000,

    [Description("Failure")]
    FAILURE = 5000,

    [Description("Not Implemented")]
    NOTIMPLEMENTED = 5001,

    [Description("Parameter incomplete")]
    PARAMETERINCOMPLETE = 5002,

    [Description("Unsupported operation")]
    UNSUPPORTED = 5003,

    [Description("AuthDefaultSource cannot be null")]
    NOAUTHSOURCE = 5004,

    [Description("Unidentified platform")]
    UNIDENTIFIEDPLATFORM = 5005,

    [Description("Illegal redirect uri")]
    ILLEGALREDIRECTURI = 5006,

    [Description("Illegal request")]
    ILLEGALREQUEST = 5007,

    [Description("Illegal code")]
    ILLEGALCODE = 5008,

    [Description("Illegal state")]
    ILLEGALSTATUS = 5009,

    [Description("The refresh token is required; it must not be null")]
    REQUIREDREFRESHTOKEN = 5010,
}