using System;

namespace NetInterop.Routing
{
    public class CancelProcessingHandler : Handler
    {
        public CancelProcessingHandler(String message)
        {
            Message = message;
        }

        public String Message { get; set; }

        protected internal override bool CheckForNext()
        {
            throw new NotImplementedException();
        }

        public override Handler Parse()
        {
            throw new NotImplementedException();
        }
    }
}