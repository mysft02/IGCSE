namespace BusinessObject.Payload.Response
{
    public interface IApiBodyProcess
    {
        IDictionary<string, IEnumerable<object>> ProcessBody();
    }
}