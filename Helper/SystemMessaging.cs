namespace CDN.Helper
{
    public enum MesagesCode
    {
        Insert,
        Update,
        Delete,
        Duplicate,
        NotFound,
        IsDefault,
        Error,
        SmsSend,
        Exception
    }

    public class SystemMessaging
    {
        public MesagesCode Code { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        //public string Exception { get; set; }
        public object? Entity { get; set; }
        public string ReturnData { get; set; }
        public SystemMessaging(MesagesCode code, string message)
        {
            Code = code;
            Message = message;

        }
        public SystemMessaging(MesagesCode code, string message, object entity)
        {
            Code = code;
            Message = message;
            Entity = entity;

        }
        public SystemMessaging(MesagesCode code, string message, object entity, string returnData)
        {
            Code = code;
            Message = message;
            Entity = entity;
            ReturnData = returnData;

        }
        //public SystemMessaging(MesagesCode code, string message, string errorCode,string exception)
        //{
        //    Code = code;
        //    Message = message;
        //    Exception = exception;
        //    ErrorCode = errorCode;
        //}

        protected SystemMessaging()
        {
        }
    }

}
