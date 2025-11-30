namespace payroll.Shared
{
    public class Result
    {
        public Result() { }

        public Result( bool issuccesFul, string message, object response)
        {
            IsSuccessful = issuccesFul;
            Message = message;
            Response = response;
        }

        public Result Success(string messsage)
        {
            IsSuccessful = true;
            Message = messsage;
            Response = null;

            return this;
        }

        public Result Success(string message, object response)
        {
            IsSuccessful = true;
            Message = message;
            Response = response;

            return this;
        }

        public Result Exception(string messsage)
        {
            IsSuccessful = false;
            Message = messsage;
            Response = null;

            return this;
        }

        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public object Response { get; set; }
    }
}
