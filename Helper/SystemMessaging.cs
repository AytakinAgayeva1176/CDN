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
        Exception
    }

    public class SystemMessaging
    {
        public MesagesCode Code { get; set; }
        public string Message { get; set; }
        public object? Entity { get; set; }
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
  

        protected SystemMessaging()
        {
        }
    }

}
