namespace WebApplication.Services
{
    public interface IAccessGuardService
    {
        public bool CanAccess(string index);
    }
}